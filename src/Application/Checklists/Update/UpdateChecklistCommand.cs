using Application.Common.DTOs;
using Application.Common.Messaging;

namespace  Application.Checklists.Update;

public sealed record UpdateChecklistCommand(
    Guid Id,
    bool isActive,
    bool isValid,
    string title,
    ICollection<ChecklistGroupDto> groups
) : ICommand; 
