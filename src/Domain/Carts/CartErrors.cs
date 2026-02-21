using SharedKernel;

namespace Domain.Carts;

/// <summary>
/// Centralized error definitions for Cart domain operations.
/// Follows domain-driven design patterns for error handling.
/// </summary>
public static class CartErrors
{
    public static Error NotFound(Guid customerId) =>
        Error.NotFound(
            "Carts.NotFound",
            $"Cart with ID '{customerId}' was not found.");

    public static Error EmailAlreadyExists(string email) =>
        Error.Conflict(
            "Carts.EmailExists",
            $"Cart with email '{email}' already exists.");

    public static Error InvalidEmail() =>
        Error.Problem(
            "Carts.InvalidEmail",
            "The provided email address is invalid.");

    public static Error InvalidName() =>
        Error.Problem(
            "Carts.InvalidName",
            "Cart name is required and must be between 1 and 200 characters.");

    public static Error CannotDeleteCart() =>
        Error.Failure(
            "Carts.CannotDelete",
            "This customer cannot be deleted due to existing orders.");
}
