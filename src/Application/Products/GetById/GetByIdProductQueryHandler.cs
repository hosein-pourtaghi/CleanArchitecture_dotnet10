
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetById;
 
internal sealed class GetByIdProductQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetByIdProductQuery, ProductDto>
{
    public async Task<Result<ProductDto>> Handle(GetByIdProductQuery query, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductDto>(ProductErrors.NotFound(query.Id));
        }

        var productDto = mapper.Map<ProductDto>(product);

        return productDto;
    }
}

