using SharedKernel;

namespace Domain.Aggregates.Products;

/// <summary>
/// Centralized error definitions for Product domain operations.
/// Follows domain-driven design patterns for error handling.
/// </summary>
public static class ProductErrors
{
    public static Error NotFound(Guid productId) =>
        Error.NotFound(
            "Products.NotFound",
            $"Product with ID '{productId}' was not found.");
     
    public static Error InvalidName() =>
        Error.Problem(
            "Products.InvalidName",
            "Product name is required and must be between 1 and 200 characters.");

    public static Error CannotDeleteProduct() =>
        Error.Failure(
            "Products.CannotDelete",
            "This product cannot be deleted due to existing carts.");
}
