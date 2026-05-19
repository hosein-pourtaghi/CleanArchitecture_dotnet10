namespace SharedKernel.Messaging;


/// <summary>
/// Marker interface for domain event handlers
/// </summary>
public interface IDomainEventHandler<in TEvent> : IDomainEventHandler where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base interface for all domain event handlers
/// </summary>
public interface IDomainEventHandler
{
    /// <summary>
    /// Determines the order in which handlers are executed.
    /// Lower numbers execute first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Whether to continue processing other handlers if this one fails
    /// </summary>
    bool ContinueOnError { get; }
}
