using SharedKernel;

namespace Domain.Customers;

/// <summary>
/// Centralized error definitions for Customer domain operations.
/// Follows domain-driven design patterns for error handling.
/// </summary>
public static class CustomerErrors
{
    public static Error NotFound(Guid customerId) =>
        Error.NotFound(
            "Customers.NotFound",
            $"Customer with ID '{customerId}' was not found.");

    public static Error EmailAlreadyExists(string email) =>
        Error.Conflict(
            "Customers.EmailExists",
            $"Customer with email '{email}' already exists.");

    public static Error InvalidEmail() =>
        Error.Problem(
            "Customers.InvalidEmail",
            "The provided email address is invalid.");

    public static Error InvalidName() =>
        Error.Problem(
            "Customers.InvalidName",
            "Customer name is required and must be between 1 and 200 characters.");

    public static Error CannotDeleteCustomer() =>
        Error.Failure(
            "Customers.CannotDelete",
            "This customer cannot be deleted due to existing orders.");
}
