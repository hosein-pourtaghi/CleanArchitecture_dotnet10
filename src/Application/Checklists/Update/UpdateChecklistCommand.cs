using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Checklists.Update;

public sealed record UpdateChecklistCommand(
    Guid Id,
    bool isActive,
    bool isValid,
    string title,
    ICollection<ChecklistGroupDto> groups
) : ICommand; 
