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
        try
        {

        var products = await context.Products
            .AsNoTracking()
            .Skip(0)
            .Take(100) // You can adjust the page size as needed
            .ToListAsync(cancellationToken);

        var productDtos = mapper.Map<List<ProductDto>>(products);

        return productDtos;
        }
        catch (Exception ee)
        {

            throw;
        }
    }
}

