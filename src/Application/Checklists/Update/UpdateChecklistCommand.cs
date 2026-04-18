using Application.Common.DTOs.Checklists;
using Application.Common.Messaging;

namespace  Application.Checklists.Update;

public sealed record UpdateChecklistCommand(
    Guid Id,
    bool isActive,
    bool isValid,
    string title,
    ICollection<ChecklistGroupDto> groups
) : ICommand; 
