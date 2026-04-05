using Application.Common.DTOs;
using Application.Common.Messaging;

namespace  Application.Checklists.Create;

public sealed record CreateChecklistCommand(
    bool isActive,
    int version,
    DateTime lastModified,
    bool isValid,
    string title,
    ICollection<ChecklistGroupDto> groups
) : ICommand<Guid>; 
