using Application.Carts.Create;
using Application.Common.DTOs;
using AutoMapper;
using Domain.Carts;

namespace Application.Common.Mappings;

/// <summary>
/// Cart profile for all entity-to-DTO mappings across the application.
/// Centralizes mapping configuration for better maintainability and consistency.
/// </summary>
public class CartProfile : Profile
{
    public CartProfile()
    {

        CreateMap<Cart, CartDto>();

        CreateMap<CreateCartCommand, Cart>();

        CreateMap<CartItem, CartItemDto>();

    }
}
