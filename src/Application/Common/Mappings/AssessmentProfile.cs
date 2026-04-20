using Application.Common.DTOs.Checklists;
using AutoMapper;
using Domain.Aggregates.Assessments;

namespace Application.Common.Mappings;
 
public class AssessmentProfile : Profile
{
    public AssessmentProfile()
    {

        CreateMap<Assessment, AssessmentDto>()
            .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers))
            ;


    }
}

