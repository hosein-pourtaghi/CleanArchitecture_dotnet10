
using Application.Common.DTOs;
using AutoMapper;
using Domain.Checklists;

namespace Application.Common.Mappings;
 
public class ChecklistProfile : Profile
{
    public ChecklistProfile()
    {

        CreateMap<Checklist, ChecklistDto>()
            ;


    }
}

