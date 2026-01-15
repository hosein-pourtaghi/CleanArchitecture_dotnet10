namespace Application.Common.DTOs;

/// <summary>
/// Data Transfer Object for Customer.
/// Used for API responses and query results.
/// </summary>
public sealed class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; } 
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
