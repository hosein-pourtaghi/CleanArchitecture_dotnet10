using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace Application.Customers.GetById;

public sealed record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerDto>;
