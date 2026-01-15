using Application.Abstractions.Messaging;

namespace Application.Customers.Copy;

/// <summary>
/// Command to Copy a new customer.
/// Returns the newly Copied customer's ID.
/// </summary>
public sealed record CopyCustomerCommand(
    string Name,
    string Email,
    string? Phone = null,
    string? Address = null) : ICommand<bool>;
