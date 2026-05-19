using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using Application.Common.DTOs.Notifications;

namespace Application.Common.Interfaces;

 
/// <summary>
/// Interface for the domain event channel used to publish notifications
/// for async processing.
/// </summary>
public interface IDomainEventChannel
{
    /// <summary>
    /// Asynchronously writes a notification to the channel.
    /// </summary>
    Task WriteAsync(DomainEventNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the channel reader for consuming notifications.
    /// </summary>
    ChannelReader<DomainEventNotification> Reader { get; }

    /// <summary>
    /// Marks the channel as complete.
    /// </summary>
    void Complete();

    /// <summary>
    /// Gets the current count of items in the channel.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Checks if the channel is empty.
    /// </summary>
    bool IsEmpty { get; }
}
