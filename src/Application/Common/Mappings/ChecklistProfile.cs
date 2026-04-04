
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
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Version, opt => opt.Ignore())
            .ForMember(dest => dest.LastModified, opt => opt.Ignore())
            .ForMember(dest => dest.IsValid, opt => opt.Ignore())
            ;

        CreateMap<ChecklistGroupDto, ChecklistGroup>()
            .ForMember(dest => dest.Checklist, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ReverseMap()
            ;
        CreateMap<ChecklistQuestionDto, ChecklistQuestion>()
            .ForMember(dest => dest.Group, opt => opt.Ignore())
            .ReverseMap()
            ;
        CreateMap<ChecklistQuestionOption, ChecklistQuestionOptionDto>()
            .ReverseMap()
            ;

        CreateMap<Checklist, Checklist>();
        CreateMap<ChecklistGroup, ChecklistGroup>();
        CreateMap<ChecklistQuestion, ChecklistQuestion>();
        CreateMap<ChecklistQuestionOption, ChecklistQuestionOption>();



    }
}

