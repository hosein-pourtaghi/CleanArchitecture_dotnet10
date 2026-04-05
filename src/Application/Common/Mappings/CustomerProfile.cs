using Application.Common.DTOs;
using AutoMapper;
using Domain.Customers;
using CommonDTOs = Application.Common.DTOs;

namespace Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for all entity-to-DTO mappings across the application.
/// Centralizes mapping configuration for better maintainability and consistency.
/// </summary>
public class CustomerProfile : Profile
{
    public CustomerProfile()
    { 
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address)) 
            ;
    }
}
