using Application.Abstractions.Messaging;
using Application.Customers.DTOs;

namespace Application.Customers.Get;

public sealed record GetCustomersQuery() : IQuery<List<CustomerDto>>;