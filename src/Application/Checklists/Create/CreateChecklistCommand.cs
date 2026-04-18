using Application.Common.DTOs.Checklists;
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
