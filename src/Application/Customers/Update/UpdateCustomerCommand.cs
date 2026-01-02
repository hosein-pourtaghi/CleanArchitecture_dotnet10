using Application.Abstractions.Messaging;

namespace Application.Customers.Update;

public sealed class UpdateCustomerCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}