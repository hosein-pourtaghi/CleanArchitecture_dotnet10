

using SharedKernel;
using SharedKernel.Messaging;

namespace Domain.Aggregates.Checklists;

/// <summary>
/// Domain event raised when a new checklist is created.
/// This event is part of the domain model and should be used for:
/// - Audit logging
/// - Real-time notifications
/// - Integration events
/// - Cache invalidation
/// </summary>
public sealed record ChecklistCreatedDomainEvent : IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event (for idempotency)
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// UTC timestamp when the event occurred
    /// </summary>
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Sequence number for ordering events from the same aggregate
    /// </summary>
    public int EventSequence { get; init; } = 1;

    // ═══════════════════════════════════════════════════════════════════════
    // Checklist Properties
    // ═══════════════════════════════════════════════════════════════════════

    public Guid ChecklistId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int Version { get; init; }
    public bool IsActive { get; init; }
    public bool IsValid { get; init; }
    public float TotalScore { get; init; }
    public int GroupCount { get; init; }

    // ═══════════════════════════════════════════════════════════════════════
    // Audit Properties
    // ═══════════════════════════════════════════════════════════════════════

    public Guid? CreatedById { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAtUtc { get; init; }

    // ═══════════════════════════════════════════════════════════════════════
    // Metadata for tracking and debugging
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Correlation ID for tracing the event through the system
    /// </summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Causation ID - links to the command that caused this event
    /// </summary>
    public Guid? CausationId { get; init; }

    /// <summary>
    /// Source application that generated this event
    /// </summary>
    public string Source { get; init; } = "ChecklistService";

    /// <summary>
    /// Event version for schema evolution
    /// </summary>
    public int EventVersion { get; init; } = 1;
}
