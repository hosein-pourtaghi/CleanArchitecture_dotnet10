using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace Application.Checklists.GetById;

/// <summary>
/// Query to retrieve a Checklist by its unique identifier.
/// Inherits from IQuery&lt;ChecklistDto&gt; which returns Result&lt;ChecklistDto&gt;.
/// Handled by <see cref="GetByIdChecklistQueryHandler"/>
/// </summary>
public sealed record GetByIdChecklistQuery(Guid Id) : IQuery<ChecklistDto>;
