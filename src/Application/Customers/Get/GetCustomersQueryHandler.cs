using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Customers.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Get;

/// <summary>
/// Handles retrieval of all customers.
/// Returns customers as transfer objects with efficient no-tracking queries.
/// </summary>
internal sealed class GetCustomersQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetCustomersQuery, List<CustomerDto>>
{
    public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery query, CancellationToken cancellationToken)
    {
        var customers = await context.Customers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var customerDtos = mapper.Map<List<CustomerDto>>(customers);

        return customerDtos;
    }
}