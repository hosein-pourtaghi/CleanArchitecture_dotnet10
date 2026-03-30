using Application.Abstractions.Data;
using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Checklists;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using Application.Common.DTOs.Shared;
using AutoMapper;
using Domain.Checklists;
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
        var res = await repository.GetAllAsync<ChecklistDto>(query.Request);
        return res;
    }
}

