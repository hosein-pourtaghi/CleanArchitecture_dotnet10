using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.DTOs.Notifications;
 

/// <summary>
/// Specific payload for checklist-related notifications
/// </summary>
public sealed class ChecklistNotificationPayload
{
    // ═══════════════════════════════════════════════════════════════════════
    // Event Identification
    // ═══════════════════════════════════════════════════════════════════════

    public required string EventType { get; init; }
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    // ═══════════════════════════════════════════════════════════════════════
    // Checklist Data
    // ═══════════════════════════════════════════════════════════════════════

    public Guid ChecklistId { get; init; }
    public required string Title { get; init; }
    public int Version { get; init; }
    public bool IsActive { get; init; }
    public bool IsValid { get; init; }
    public float TotalScore { get; init; }
    public int GroupCount { get; init; }

    // ═══════════════════════════════════════════════════════════════════════
    // Audit
    // ═══════════════════════════════════════════════════════════════════════

    public Guid? CreatedById { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAtUtc { get; init; }

    // ═══════════════════════════════════════════════════════════════════════
    // Conversion to DomainEventNotification
    // ═══════════════════════════════════════════════════════════════════════

    public DomainEventNotification ToNotification()
    {
        return new DomainEventNotification
        {
            EventType = EventType,
            EventId = EventId,
            OccurredAtUtc = OccurredAtUtc,
            CorrelationId = CorrelationId,
            EntityId = ChecklistId,
            EntityType = "Checklist",
            Payload = System.Text.Json.JsonSerializer.Serialize(this),
            Target = DetermineTarget(),
            TargetUserIds = GetTargetUserIds(),
            TargetRoles = GetTargetRoles(),
            Priority = DeterminePriority(),
            ExpiresAtUtc = OccurredAtUtc.AddDays(7)
        };
    }

    private NotificationTarget DetermineTarget()
    {
        // Notify admins for checklist creation
        return NotificationTarget.Admins;
    }

    private IReadOnlyList<Guid>? GetTargetUserIds()
    {
        if (CreatedById.HasValue)
        {
            return [CreatedById.Value];
        }
        return null;
    }

    private IReadOnlyList<string>? GetTargetRoles()
    {
        return ["Admin", "ChecklistManager"];
    }

    private NotificationPriority DeterminePriority()
    {
        return EventType.Contains("Created")
            ? NotificationPriority.Normal
            : NotificationPriority.High;
    }
}
