using Application.Common.Messaging;

namespace Application.Customers.Delete;

public sealed record DeleteCustomerCommand(Guid Id) : ICommand;