// NotificationApi.Contracts/DTOs/DomainEventNotificationDto.cs
namespace NotificationAPI.Dtos;

/// <summary>
/// Unified notification payload received from domain services
/// </summary>
public sealed class DomainEventNotificationDto
{
    // ═══════════════════════════════════════════════════════════════════════
    // Event Identification
    // ═══════════════════════════════════════════════════════════════════════

    public required string EventType { get; init; }
    public Guid EventId { get; init; }
    public DateTime OccurredAtUtc { get; init; }

    // ═══════════════════════════════════════════════════════════════════════
    // Correlation & Tracing
    // ═══════════════════════════════════════════════════════════════════════

    public Guid CorrelationId { get; init; }
    public Guid? CausationId { get; init; }

    // ═══════════════════════════════════════════════════════════════════════
    // Entity Information
    // ═══════════════════════════════════════════════════════════════════════

    public Guid EntityId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string? Payload { get; init; }

    // ═══════════════════════════════════════════════════════════════════════
    // Routing
    // ═══════════════════════════════════════════════════════════════════════

    public string Target { get; init; } = "All";
    public List<Guid>? TargetUserIds { get; init; }
    public List<string>? TargetRoles { get; init; }

    // ═══════════════════════════════════════════════════════════════════════
    // Priority & Delivery
    // ═══════════════════════════════════════════════════════════════════════

    public string Priority { get; init; } = "Normal";
    public int DeliveryAttempts { get; set; }
    public DateTime? ExpiresAtUtc { get; init; }
}
