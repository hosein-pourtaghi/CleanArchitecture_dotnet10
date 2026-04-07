using Application.Common.Data;
using Application.Common.DTOs;
using Application.Common.DTOs.Shared;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Checklists;
using Application.Common.Messaging;
using AutoMapper;
using Domain.Entities.Checklists;
using SharedKernel;

namespace Application.Checklists.GetAll;

internal sealed class GetAllChecklistQueryHandler(
    IApplicationDbContext context,
    IChecklistRepository repository,
    IMapper mapper)
    : IQueryHandler<GetAllChecklistQuery, PaginatedResult<ChecklistDto>>
{
    public async Task<Result<PaginatedResult<ChecklistDto>>> Handle(GetAllChecklistQuery query, CancellationToken cancellationToken)
    {
        var res = await repository.GetAllAsync<ChecklistDto>(query.Request,cancellationToken);
        return res;
    }
}

