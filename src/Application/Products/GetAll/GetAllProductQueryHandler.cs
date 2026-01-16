using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetAll;
 
internal sealed class GetAllProductQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetAllProductQuery, List<ProductDto>>
{
    public async Task<Result<List<ProductDto>>> Handle(GetAllProductQuery query, CancellationToken cancellationToken)
    {
        var products = await context.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var productDtos = mapper.Map<List<ProductDto>>(products);

        return productDtos;
    }
}

