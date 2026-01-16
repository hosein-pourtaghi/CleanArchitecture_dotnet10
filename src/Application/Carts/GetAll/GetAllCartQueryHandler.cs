using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Carts.Get;

/// <summary>
/// Handles retrieval of all carts.
/// Returns carts as transfer objects with efficient no-tracking queries.
/// </summary>
internal sealed class GetAllCartQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetAllCartQuery, List<CartDto>>
{
    public async Task<Result<List<CartDto>>> Handle(GetAllCartQuery query, CancellationToken cancellationToken)
    {
        var carts = await context.Carts
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var cartDtos = mapper.Map<List<CartDto>>(carts);

        return cartDtos;
    }
}
