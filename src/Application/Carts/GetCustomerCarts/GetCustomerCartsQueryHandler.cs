using System.Diagnostics;
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
        var watch = new Stopwatch();
        watch.Start();
        var q1 = context.Carts
            .AsNoTracking()
            .Where(x => x.CustomerId == query.customerId)
            .Include(x => x.CartItems)
            .ProjectTo<CartDto>(mapper.ConfigurationProvider)
            .Skip(query.page * query.pageSize)
            .Take(query.pageSize);
        var cartDtos = await q1
             .ToListAsync(cancellationToken)
             ;
        watch.Stop();
        TimeSpan a = watch.Elapsed;

        watch.Restart();
        var q2 = context.Carts
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => x.CustomerId == query.customerId)
            .Include(x => x.CartItems)
            .ProjectTo<CartDto>(mapper.ConfigurationProvider)
            .Skip(query.page * query.pageSize)
            .Take(query.pageSize)
            ;
        var cartSplitDtos = await q2
            .ToListAsync(cancellationToken)
            ;
        watch.Stop();
        TimeSpan b = watch.Elapsed;

        return cartDtos;
    }
}
