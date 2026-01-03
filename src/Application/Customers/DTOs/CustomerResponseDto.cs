namespace Application.Customers.DTOs;

/// <summary>
/// Comprehensive response DTO for customer operations.
/// Used for API responses that need to return full customer details.
/// </summary>
public sealed class CustomerResponseDto
{
    /// <summary>Unique identifier of the customer.</summary>
    public Guid Id { get; set; }

    /// <summary>Customer's full name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Customer's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Customer's phone number (optional).</summary>
    public string? Phone { get; set; }

    /// <summary>Customer's address (optional).</summary>
    public string? Address { get; set; }

    /// <summary>Timestamp when the customer record was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Timestamp when the customer record was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }
}
