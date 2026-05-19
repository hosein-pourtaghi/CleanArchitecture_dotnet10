// NotificationApi.Api/Controllers/NotificationsController.cs
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class Notifications2Controller : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<Notifications2Controller> _logger;

    public Notifications2Controller(
        INotificationService notificationService,
        ILogger<Notifications2Controller> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Receives domain events from other services and broadcasts them via SignalR
    /// </summary>
    [HttpPost("events")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReceiveEvent(
        [FromBody] DomainEventNotificationDto notification,
        CancellationToken cancellationToken)
    {
        // Validate API key
        if (!ValidateApiKey())
        {
            return Unauthorized(new { error = "Invalid API key" });
        }

        // Validate notification
        if (notification == null || string.IsNullOrEmpty(notification.EventType))
        {
            return BadRequest(new { error = "Invalid notification payload" });
        }

        // Log correlation
        var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();
        _logger.LogInformation(
            "Received notification: Type={EventType}, Id={EventId}, CorrelationId={CorrelationId}",
            notification.EventType,
            notification.EventId,
            correlationId);

        try
        {
            // Broadcast via SignalR
            await _notificationService.BroadcastAsync(notification, cancellationToken);

            return Accepted(new
            {
                message = "Notification accepted for processing",
                eventId = notification.EventId,
                correlationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process notification: {EventId}", notification.EventId);
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }

    /// <summary>
    /// Receives batch of domain events
    /// </summary>
    [HttpPost("events/batch")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReceiveBatchEvents(
        [FromBody] IReadOnlyList<DomainEventNotificationDto> notifications,
        CancellationToken cancellationToken)
    {
        if (!ValidateApiKey())
        {
            return Unauthorized(new { error = "Invalid API key" });
        }

        if (notifications == null || notifications.Count == 0)
        {
            return BadRequest(new { error = "No notifications provided" });
        }

        _logger.LogInformation("Received batch of {Count} notifications", notifications.Count);

        var tasks = notifications.Select(n =>
            _notificationService.BroadcastAsync(n, cancellationToken));

        await Task.WhenAll(tasks);

        return Accepted(new
        {
            message = $"Accepted {notifications.Count} notifications",
            count = notifications.Count
        });
    }

    private bool ValidateApiKey()
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        var expectedKey = Environment.GetEnvironmentVariable("NOTIFICATION_API_KEY");

        return !string.IsNullOrEmpty(apiKey) &&
               !string.IsNullOrEmpty(expectedKey) &&
               apiKey == expectedKey;
    }
}
