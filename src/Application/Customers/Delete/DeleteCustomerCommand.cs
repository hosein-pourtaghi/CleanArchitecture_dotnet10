using Application.Abstractions.Messaging;

namespace Application.Customers.Delete;

public sealed record DeleteCustomerCommand(Guid Id) : ICommand;