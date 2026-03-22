
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using Application.Common.DTOs.Shared;

namespace  Application.Checklists.GetAll;
 
public sealed record GetAllChecklistQuery(PaginatedRequest Request) : IQuery<PaginatedResult<ChecklistDto>>;

