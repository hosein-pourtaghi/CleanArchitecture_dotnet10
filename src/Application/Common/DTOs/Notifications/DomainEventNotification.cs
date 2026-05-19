using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.DTOs.Notifications;
 
/// <summary>
/// Unified notification payload for all domain events.
/// This DTO is serialized and sent to the NotificationAPI.
/// </summary>
public sealed class DomainEventNotification
{
    // ═══════════════════════════════════════════════════════════════════════
    // Event Identification
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Type of the event (e.g., "ChecklistCreated", "ChecklistUpdated")
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    /// Unique identifier for this notification
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// UTC timestamp when the event occurred
    /// </summary>
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;

    // ═══════════════════════════════════════════════════════════════════════
    // Correlation & Tracing
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Correlation ID for distributed tracing
    /// </summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Causation ID - links to the command that caused this event
    /// </summary>
    public Guid? CausationId { get; init; }

    // ═══════════════════════════════════════════════════════════════════════
    // Event-Specific Data (polymorphic via JSON)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The entity ID this event relates to
    /// </summary>
    public Guid EntityId { get; init; }

    /// <summary>
    /// Type of the entity (e.g., "Checklist", "Assessment")
    /// </summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>
    /// Additional event-specific data as JSON
    /// </summary>
    public string? Payload { get; init; }

    // ═══════════════════════════════════════════════════════════════════════
    // Target & Routing
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Target audience for this notification
    /// </summary>
    public NotificationTarget Target { get; init; } = NotificationTarget.All;

    /// <summary>
    /// Specific user IDs to notify (for targeted notifications)
    /// </summary>
    public IReadOnlyList<Guid>? TargetUserIds { get; init; }

    /// <summary>
    /// Specific roles to notify (for role-based notifications)
    /// </summary>
    public IReadOnlyList<string>? TargetRoles { get; init; }

    // ═══════════════════════════════════════════════════════════════════════
    // Priority & Delivery
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Notification priority level
    /// </summary>
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;

    /// <summary>
    /// Number of delivery attempts made
    /// </summary>
    public int DeliveryAttempts { get; set; }

    /// <summary>
    /// When the notification expires (null = never)
    /// </summary>
    public DateTime? ExpiresAtUtc { get; init; }
}

/// <summary>
/// Notification target audience
/// </summary>
public enum NotificationTarget
{
    /// <summary>All connected users</summary>
    All,

    /// <summary>Only administrators</summary>
    Admins,

    /// <summary>Specific users only</summary>
    SpecificUsers,

    /// <summary>Users with specific roles</summary>
    SpecificRoles,

    /// <summary>Users related to the entity (e.g., creator, assignees)</summary>
    RelatedUsers
}

/// <summary>
/// Notification priority levels
/// </summary>
public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}
