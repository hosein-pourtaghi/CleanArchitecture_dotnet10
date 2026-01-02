using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Customers.DTOs;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Get;

internal sealed class GetCustomersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetCustomersQuery, List<CustomerDto>>
{
    public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery query, CancellationToken cancellationToken)
    {
        var customers = await context.Customers
            .AsNoTracking()
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return customers;
    }
}