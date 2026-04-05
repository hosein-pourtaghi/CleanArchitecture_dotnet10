using SharedKernel.BaseEntities;

namespace Domain.Customers;

public sealed class Customer : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}
