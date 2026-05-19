// Infrastructure/Messaging/DomainEventDispatcherService.cs
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Common.DTOs.Notifications;
using Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.DomainEvents;

/// <summary>
/// Background service that consumes domain events from the channel
/// and sends them to the NotificationAPI for real-time delivery.
///
/// This service runs as a singleton and uses the channel for
/// lock-free, high-performance communication with scoped services.
/// </summary>
public sealed class DomainEventDispatcherService : BackgroundService
{
    private readonly IDomainEventChannel _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DomainEventDispatcherService> _logger;
    private readonly NotificationApiOptions _options;

    // Circuit breaker state
    private readonly SemaphoreSlim _circuitBreaker = new(1, 1);
    private DateTime _circuitOpenedAt = DateTime.MinValue;
    private const int CircuitBreakerDurationSeconds = 30;
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 1000;

    public DomainEventDispatcherService(
        IDomainEventChannel channel,
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        IOptions<NotificationApiOptions> options,
        ILogger<DomainEventDispatcherService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "DomainEventDispatcherService starting. NotificationAPI: {ApiUrl}",
            _options.BaseUrl);

        // Warm up the HTTP client
        await WarmUpHttpClientAsync(stoppingToken);

        // Process events from the channel
        await foreach (var notification in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessNotificationAsync(notification, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process notification: {EventType} - {EventId}",
                    notification.EventType,
                    notification.EventId);

                // Continue processing other notifications
            }
        }

        _logger.LogInformation("DomainEventDispatcherService stopped");
    }

    private async Task ProcessNotificationAsync(
        DomainEventNotification notification,
        CancellationToken cancellationToken)
    {
        // Check circuit breaker
        if (IsCircuitOpen())
        {
            _logger.LogWarning(
                "Circuit breaker is open. Requeueing notification: {EventId}",
                notification.EventId);

            // Requeue with delay
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            await _channel.WriteAsync(notification, cancellationToken);
            return;
        }

        var retryCount = 0;

        while (retryCount < MaxRetries)
        {
            try
            {
                await SendToNotificationApiAsync(notification, cancellationToken);

                _logger.LogInformation(
                    "Notification sent successfully: {EventType} - {EventId}",
                    notification.EventType,
                    notification.EventId);

                return;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                retryCount++;
                _logger.LogWarning(
                    "NotificationAPI unavailable, retry {RetryCount}/{MaxRetries}: {EventId}",
                    retryCount, MaxRetries, notification.EventId);

                if (retryCount < MaxRetries)
                {
                    await Task.Delay(RetryDelayMs * retryCount, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send notification after {MaxRetries} retries: {EventId}",
                    MaxRetries, notification.EventId);

                // Open circuit breaker
                OpenCircuit();

                // Log for manual intervention / dead letter queue
                await LogFailedNotificationAsync(notification, ex, cancellationToken);

                return;
            }
        }
    }

    private async Task SendToNotificationApiAsync(
        DomainEventNotification notification,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("NotificationApi");

        var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoints.Notifications)
        {
            Content = JsonContent.Create(notification, options: new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        };

        // Add correlation headers for distributed tracing
        request.Headers.Add("X-Correlation-Id", notification.CorrelationId.ToString());
        request.Headers.Add("X-Event-Id", notification.EventId.ToString());
        request.Headers.Add("X-Event-Type", notification.EventType);

        var response = await client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private bool IsCircuitOpen()
    {
        if (_circuitOpenedAt == DateTime.MinValue)
            return false;

        var elapsed = DateTime.UtcNow - _circuitOpenedAt;
        if (elapsed.TotalSeconds > CircuitBreakerDurationSeconds)
        {
            // Reset circuit breaker
            _circuitOpenedAt = DateTime.MinValue;
            _logger.LogInformation("Circuit breaker reset");
            return false;
        }

        return true;
    }

    private void OpenCircuit()
    {
        _circuitBreaker.Wait();
        try
        {
            _circuitOpenedAt = DateTime.UtcNow;
            _logger.LogWarning("Circuit breaker opened");
        }
        finally
        {
            _circuitBreaker.Release();
        }
    }

    private async Task WarmUpHttpClientAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NotificationApi");

            // Send a health check ping
            var response = await client.GetAsync(_options.Endpoints.Health, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("NotificationAPI health check passed");
            }
            else
            {
                _logger.LogWarning(
                    "NotificationAPI health check failed: {StatusCode}",
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NotificationAPI warmup failed - will retry on first event");
        }
    }

    private async Task LogFailedNotificationAsync(
        DomainEventNotification notification,
        Exception exception,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DomainEventDispatcherService>>();

        logger.LogError(
            exception,
            "Dead letter notification: Type={EventType}, Id={EventId}, EntityId={EntityId}, CorrelationId={CorrelationId}",
            notification.EventType,
            notification.EventId,
            notification.EntityId,
            notification.CorrelationId);

        // Here you could also:
        // 1. Write to a dead letter table in the database
        // 2. Send to a message queue for later processing
        // 3. Send an alert to monitoring systems
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DomainEventDispatcherService stopping...");

        // Complete the channel to signal no more writes
        _channel.Complete();

        await base.StopAsync(cancellationToken);
    }
}
