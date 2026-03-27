using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Assessments.GetAll;
 
internal sealed class GetAllAssessmentQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetAllAssessmentQuery, List<AssessmentDto>>
{
    public async Task<Result<List<AssessmentDto>>> Handle(GetAllAssessmentQuery query, CancellationToken cancellationToken)
    {
        var assessments = await context.Assessments
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var assessmentDtos = mapper.Map<List<AssessmentDto>>(assessments);

        return assessmentDtos;
    }
}

