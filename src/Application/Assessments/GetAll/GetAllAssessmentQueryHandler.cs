using Application.Common.DTOs.Checklists;
using Application.Common.Interfaces.Core;
using Application.Common.Messaging;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Exceptions;

namespace Application.Assessments.GetAll;
 
internal sealed class GetAllAssessmentQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetAllAssessmentQuery, List<AssessmentDto>>
{
    public async Task<Result<List<AssessmentDto>>> Handle(GetAllAssessmentQuery query, CancellationToken cancellationToken)
    {
        throw new Exception("sdfsd");
        //throw new BusinessRuleException("sdf", "sdf");

        var assessments = await context.Assessments
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var assessmentDtos = mapper.Map<List<AssessmentDto>>(assessments);

        return assessmentDtos;
    }
}

