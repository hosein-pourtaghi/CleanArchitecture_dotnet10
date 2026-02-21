using SharedKernel;

namespace Domain.Customers;

/// <summary>
/// Domain event raised when a customer is deleted.
/// Contains comprehensive information for auditing, logging, and async operations via message bus.
/// </summary>
public sealed class CustomerDeletedDomainEvent(
    Guid customerId,
    string name,
    string email,
    string? phone = null,
    string? address = null) : IDomainEvent
{
    /// <summary>Unique identifier of the deleted customer.</summary>
    public Guid CustomerId { get; } = customerId;

    /// <summary>Customer's name at time of deletion.</summary>
    public string Name { get; } = name;

    /// <summary>Customer's email address at time of deletion.</summary>
    public string Email { get; } = email;

    /// <summary>Customer's phone number at time of deletion (optional).</summary>
    public string? Phone { get; } = phone;

    /// <summary>Customer's address at time of deletion (optional).</summary>
    public string? Address { get; } = address;

    /// <summary>Timestamp when the event occurred.</summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>Event identifier for tracking and idempotency.</summary>
    public Guid EventId { get; } = Guid.NewGuid();
}
