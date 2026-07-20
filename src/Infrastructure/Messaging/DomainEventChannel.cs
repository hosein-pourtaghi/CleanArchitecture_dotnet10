// Infrastructure/Messaging/DomainEventChannel.cs
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Application.Common.Interfaces;
using Application.Common.DTOs.Notifications;


namespace Infrastructure.Messaging;
/// <summary>
/// Channel-based producer for domain events.
/// Uses System.Threading.Channels for high-performance, lock-free communication
/// between the scoped domain event dispatcher and the singleton background service.
/// </summary>
public sealed class DomainEventChannel : IDomainEventChannel
{
    private readonly Channel<DomainEventNotification> _channel;
    private readonly ILogger<DomainEventChannel> _logger;

    // Bounded channel with backpressure to prevent memory issues
    private const int MaxQueueSize = 10_000;
    private const int MaxRetries = 3;

    public DomainEventChannel(ILogger<DomainEventChannel> logger)
    {
        _logger = logger;

        // Create bounded channel with overflow strategy
        _channel = Channel.CreateBounded<DomainEventNotification>(
            new BoundedChannelOptions(MaxQueueSize)
            {
                // Allow readers to wait when queue is empty
                SingleReader = false,
                SingleWriter = false,

                // Drop oldest if full (can be changed to WaitToWrite for backpressure)
                FullMode = BoundedChannelFullMode.DropOldest,

                // Enable statistics via performance counters
                AllowSynchronousContinuations = false
            });
    }

    /// <summary>
    /// Asynchronously writes a notification to the channel with retry logic.
    /// </summary>
    public async Task WriteAsync(
        DomainEventNotification notification,
        CancellationToken cancellationToken = default)
    {
        var retryCount = 0;
        var exceptionList = new List<Exception>();

        while (retryCount < MaxRetries)
        {
            try
            {
                // Try to write with a timeout to prevent indefinite blocking
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                await _channel.Writer.WriteAsync(notification, cts.Token);

                _logger.LogDebug(
                    "Notification written to channel: {EventType} - {EventId}",
                    notification.EventType,
                    notification.EventId);

                return;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Timeout occurred, retry
                retryCount++;
                _logger.LogWarning(
                    "Channel write timeout, retry {RetryCount}/{MaxRetries}: {EventType}",
                    retryCount, MaxRetries, notification.EventType);

                await Task.Delay(TimeSpan.FromMilliseconds(100 * retryCount), cancellationToken);
            }
            catch (ChannelClosedException)
            {
                _logger.LogError(
                    "Channel is closed, cannot write notification: {EventType}",
                    notification.EventType);
                throw;
            }
            catch (Exception ex)
            {
                exceptionList.Add(ex);
                retryCount++;

                _logger.LogWarning(
                    ex,
                    "Channel write failed, retry {RetryCount}/{MaxRetries}: {EventType}",
                    retryCount, MaxRetries, notification.EventType);

                if (retryCount < MaxRetries)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * retryCount), cancellationToken);
                }
            }
        }

        // All retries exhausted
        throw new InvalidOperationException(
            $"Failed to write notification after {MaxRetries} retries",
            exceptionList.FirstOrDefault());
    }

    /// <summary>
    /// Gets the reader for consuming notifications.
    /// </summary>
    public ChannelReader<DomainEventNotification> Reader => _channel.Reader;

    /// <summary>
    /// Marks the channel as complete. No more items can be written.
    /// </summary>
    public void Complete() => _channel.Writer.Complete();

    /// <summary>
    /// Gets the current count of items in the channel.
    /// </summary>
    public int Count => _channel.Reader.Count;

    /// <summary>
    /// Checks if the channel is empty.
    /// </summary>
    public bool IsEmpty => _channel.Reader.Count == 0;
}
