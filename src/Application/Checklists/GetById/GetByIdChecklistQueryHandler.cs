using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Domain.Checklists;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Checklists.GetById;

/// <summary>
/// Handles <see cref="GetByIdChecklistQuery"/> requests.
/// Retrieves a single Checklist by ID with no-tracking for read-only access.
/// Returns <see cref="Result{TValue}"/> containing the DTO or a failure result if not found.
/// </summary>
internal sealed class GetByIdChecklistQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetByIdChecklistQuery, ChecklistDto>
{
    public async Task<Result<ChecklistDto>> Handle(
        GetByIdChecklistQuery query,
        CancellationToken cancellationToken)
    {
        var checklist = await context.Checklists
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        if (checklist is null)
        {
            return Result.Failure<ChecklistDto>(
                ChecklistErrors.NotFound(query.Id));
        }

        var checklistDto = mapper.Map<ChecklistDto>(
            checklist);

        return checklistDto;
    }
}
