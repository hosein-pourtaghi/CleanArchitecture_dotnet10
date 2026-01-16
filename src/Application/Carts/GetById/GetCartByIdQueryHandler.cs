using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Domain.Carts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Carts.GetById;

/// <summary>
/// Handles fetching a single cart by ID.
/// Uses AutoMapper for efficient entity-to-DTO mapping and AsNoTracking for read-only queries.
/// </summary>
internal sealed class GetCartByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetCartByIdQuery, CartDto>
{
    public async Task<Result<CartDto>> Handle(GetCartByIdQuery query, CancellationToken cancellationToken)
    {
        var cart = await context.Carts
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        if (cart is null)
        {
            return Result.Failure<CartDto>(CartErrors.NotFound(query.Id));
        }

        var cartDto = mapper.Map<CartDto>(cart);

        return cartDto;
    }
}
