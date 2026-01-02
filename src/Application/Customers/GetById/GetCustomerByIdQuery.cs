using Application.Abstractions.Messaging;
using Application.Customers.DTOs;

namespace Application.Customers.GetById;

public sealed record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerDto>;