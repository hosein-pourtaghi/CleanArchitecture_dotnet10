using Application.Common.Data;
using Application.Common.DTOs;
using Application.Common.Messaging;
using AutoMapper;
using Domain.Entities.Assessments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Assessments.GetById;

/// <summary>
/// Handles <see cref="GetByIdAssessmentQuery"/> requests.
/// Retrieves a single Assessment by ID with no-tracking for read-only access.
/// Returns <see cref="Result{TValue}"/> containing the DTO or a failure result if not found.
/// </summary>
internal sealed class GetByIdAssessmentQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetByIdAssessmentQuery, AssessmentDto>
{
    public async Task<Result<AssessmentDto>> Handle(
        GetByIdAssessmentQuery query,
        CancellationToken cancellationToken)
    {
        var assessment = await context.Assessments
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<AssessmentDto>(
                AssessmentErrors.NotFound(query.Id));
        }

        var assessmentDto = mapper.Map<AssessmentDto>(
            assessment);

        return assessmentDto;
    }
}
