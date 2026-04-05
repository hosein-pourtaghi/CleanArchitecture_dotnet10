using Application.Common.Messaging;

namespace Application.Customers.Create;

/// <summary>
/// Command to create a new customer.
/// Returns the newly created customer's ID.
/// </summary>
public sealed record CreateCustomerCommand(
    string Name,
    string Email,
    string? Phone = null,
    string? Address = null) : ICommand<Guid>;
