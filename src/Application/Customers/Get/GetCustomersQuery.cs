using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace Application.Customers.Get;

public sealed record GetCustomersQuery() : IQuery<List<CustomerDto>>;
