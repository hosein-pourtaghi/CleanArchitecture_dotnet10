using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Carts.GetCustomerCarts;

/// <summary>
/// Handles retrieval of all carts.
/// Returns carts as transfer objects with efficient no-tracking queries.
/// </summary>
internal sealed class GetCustomerCartsQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetCustomerCartsQuery, List<CartDto>>
{
    public async Task<Result<List<CartDto>>> Handle(GetCustomerCartsQuery query, CancellationToken cancellationToken)
    {
        var cartDtos = await context.Carts
            .AsNoTracking()
            .Where(x => x.CustomerId == query.customerId)
            .Include(x => x.CartItems.Take(3).Where(x => x.Quantity > 200))
            .ProjectTo<CartDto>(mapper.ConfigurationProvider)
            .Skip(query.page * query.pageSize)
            .Take(query.pageSize)
            .ToListAsync(cancellationToken)
            ;

        return cartDtos;
    }
}
