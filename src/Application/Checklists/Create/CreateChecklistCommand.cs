using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Checklists.Create;

public sealed record CreateChecklistCommand(
    bool isActive,
    int version,
    DateTime lastModified,
    bool isValid,
    string title,
    ICollection<ChecklistGroupDto> groups,
    ICollection<ChecklistQuestionDto> questions
) : ICommand<Guid>; 
