namespace Application.Customers.Create;

/// <summary>
/// Response DTO returned after successfully creating a customer.
/// Contains the ID of the newly created customer.
/// </summary>
public sealed class CreateCustomerResponse
{
    /// <summary>The unique identifier of the created customer.</summary>
    public Guid Id { get; set; }
}
