using Application.Common.DTOs;
using Application.Common.Messaging;

namespace Application.Customers.GetById;

public sealed record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerDto>;
