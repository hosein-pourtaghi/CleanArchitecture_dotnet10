using Application.Common.DTOs;
using Application.Common.Messaging;

namespace Application.Customers.Get;

public sealed record GetCustomersQuery() : IQuery<List<CustomerDto>>;
