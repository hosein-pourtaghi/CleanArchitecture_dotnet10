using SharedKernel;

namespace Domain.Entities.Customers;

/// <summary>
/// Domain event raised when a customer is created.
/// Contains comprehensive information for auditing, logging, and async operations via message bus.
/// </summary>
public sealed class CustomerCreatedDomainEvent(
    Guid customerId,
    string name,
    string email,
    string? phone = null,
    string? address = null) : IDomainEvent
{
    /// <summary>Unique identifier of the created customer.</summary>
    public Guid CustomerId { get; } = customerId;

    /// <summary>Customer's full name.</summary>
    public string Name { get; } = name;

    /// <summary>Customer's email address.</summary>
    public string Email { get; } = email;

    /// <summary>Customer's phone number (optional).</summary>
    public string? Phone { get; } = phone;

    /// <summary>Customer's address (optional).</summary>
    public string? Address { get; } = address;

    /// <summary>Timestamp when the event occurred.</summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>Event identifier for tracking and idempotency.</summary>
    public Guid EventId { get; } = Guid.NewGuid();
}
