using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Customers.DTOs;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.GetById;

internal sealed class GetCustomerByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetCustomerByIdQuery, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .Where(c => c.Id == query.Id)
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
            .SingleOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerDto>(Error.NotFound("Customers.NotFound", $"Customer with Id '{query.Id}' was not found"));
        }

        return customer;
    }
}