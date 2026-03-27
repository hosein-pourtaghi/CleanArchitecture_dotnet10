
using Application.Checklists.Create;
using Application.Checklists.Update;
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
        CreateMap<CreateChecklistCommand, Checklist>()
            ;
        CreateMap<UpdateChecklistCommand, Checklist>()
            ;

        CreateMap<ChecklistGroupDto, ChecklistGroup>()
            .ForMember(dest => dest.Checklist, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            ;
        CreateMap<ChecklistQuestionDto, ChecklistQuestion>()
            .ForMember(dest => dest.Group, opt => opt.Ignore())
            ;

    }
}

