using Application.Abstractions.Messaging;

namespace Application.Customers.Create;

public sealed class CreateCustomerCommand : ICommand<Guid>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}