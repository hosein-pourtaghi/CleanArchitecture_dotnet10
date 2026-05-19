// Application/Checklists/Events/ChecklistCreatedEventHandler.cs
using Application.Common.DTOs.Notifications;
using Application.Common.Interfaces;
using Domain.Aggregates.Checklists;
using Microsoft.Extensions.Logging;
using SharedKernel.Messaging;

namespace Application.Checklists.Create;


/// <summary>
/// Handles ChecklistCreatedDomainEvent by publishing to the domain event channel
/// for async processing by the background service.
/// 
/// This handler is responsible ONLY for capturing the event and placing it
/// in the channel. Actual notification delivery is handled by DomainEventDispatcherService.
/// </summary>
public sealed class ChecklistCreatedEventHandler : IDomainEventHandler<ChecklistCreatedDomainEvent>
{
    private readonly IDomainEventChannel _domainEventChannel;
    private readonly ILogger<ChecklistCreatedEventHandler> _logger;

    public int Order => 1; // Execute early
    public bool ContinueOnError => true;

    public ChecklistCreatedEventHandler(
        IDomainEventChannel domainEventChannel,
        ILogger<ChecklistCreatedEventHandler> logger)
    {
        _domainEventChannel = domainEventChannel;
        _logger = logger;
    }

    public async Task HandleAsync(
        ChecklistCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "ChecklistCreated event received: {ChecklistId}, Title: {Title}, CorrelationId: {CorrelationId}",
                domainEvent.ChecklistId,
                domainEvent.Title,
                domainEvent.CorrelationId);

            // Create a serializable notification payload
            var notification = new ChecklistNotificationPayload
            {
                EventType = "ChecklistCreated",
                EventId = domainEvent.EventId,
                ChecklistId = domainEvent.ChecklistId,
                Title = domainEvent.Title,
                Version = domainEvent.Version,
                IsActive = domainEvent.IsActive,
                IsValid = domainEvent.IsValid,
                TotalScore = domainEvent.TotalScore,
                GroupCount = domainEvent.GroupCount,
                CreatedById = domainEvent.CreatedById,
                CreatedByName = domainEvent.CreatedByName,
                CreatedAtUtc = domainEvent.CreatedAtUtc,
                CorrelationId = domainEvent.CorrelationId,
                OccurredAtUtc = domainEvent.OccurredAtUtc
            };

            // Write to channel for async processing
            await _domainEventChannel.WriteAsync(notification, cancellationToken);

            _logger.LogDebug(
                "ChecklistCreated event published to channel: {ChecklistId}",
                domainEvent.ChecklistId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish ChecklistCreated event to channel: {ChecklistId}",
                domainEvent.ChecklistId);

            // Re-throw to allow retry or dead-letter handling
            throw;
        }
    }
}
