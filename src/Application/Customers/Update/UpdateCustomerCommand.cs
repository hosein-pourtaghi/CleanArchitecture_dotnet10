using Application.Common.Messaging;

namespace Application.Customers.Update;

/// <summary>
/// Command to update an existing customer.
/// No return value - operation either succeeds or fails.
/// </summary>
public sealed record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string Email,
    string? Phone = null,
    string? Address = null) : ICommand;
