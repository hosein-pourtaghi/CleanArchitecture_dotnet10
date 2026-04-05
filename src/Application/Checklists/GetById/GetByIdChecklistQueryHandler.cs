using Application.Common.Data;
using Application.Common.DTOs;
using Application.Common.Interfaces.Checklists;
using Application.Common.Messaging;
using AutoMapper;
using Domain.Checklists;
using SharedKernel;

namespace Application.Checklists.GetById;

/// <summary>
/// Handles <see cref="GetByIdChecklistQuery"/> requests.
/// Retrieves a single Checklist by ID with no-tracking for read-only access.
/// Returns <see cref="Result{TValue}"/> containing the DTO or a failure result if not found.
/// </summary>
internal sealed class GetByIdChecklistQueryHandler(
    IApplicationDbContext context,
    IChecklistRepository repository,
    IMapper mapper)
    : IQueryHandler<GetByIdChecklistQuery, ChecklistDto>
{
    public async Task<Result<ChecklistDto>> Handle(
        GetByIdChecklistQuery query,
        CancellationToken cancellationToken)
    {
        var checklist = await repository.GetByIdAsync(query.Id);
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
