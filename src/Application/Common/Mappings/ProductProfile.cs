using Application.Common.DTOs;
using AutoMapper;
using Domain.Products;

namespace Application.Common.Mappings;

/// <summary>
/// Product profile for all entity-to-DTO mappings across the application.
/// Centralizes mapping configuration for better maintainability and consistency.
/// </summary>
public class ProductProfile : Profile
{
    public ProductProfile()
    {

        CreateMap<Product, ProductDto>();


    }
}
