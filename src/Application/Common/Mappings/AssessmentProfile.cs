using Application.Common.DTOs;
using AutoMapper;
using Domain.Entities.Assessments;

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

