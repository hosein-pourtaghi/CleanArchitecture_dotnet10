
using Application.Common.DTOs;
using Application.Common.DTOs.Shared;
using Application.Common.Messaging;

namespace  Application.Checklists.GetAll;
 
public sealed record GetAllChecklistQuery(PaginatedRequest Request) : IQuery<PaginatedResult<ChecklistDto>>;

