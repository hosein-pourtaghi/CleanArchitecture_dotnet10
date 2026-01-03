using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Customers.DTOs;
using AutoMapper;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.GetById;

/// <summary>
/// Handles fetching a single customer by ID.
/// Uses AutoMapper for efficient entity-to-DTO mapping and AsNoTracking for read-only queries.
/// </summary>
internal sealed class GetCustomerByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetCustomerByIdQuery, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerDto>(CustomerErrors.NotFound(query.Id));
        }

        var customerDto = mapper.Map<CustomerDto>(customer);

        return customerDto;
    }
}