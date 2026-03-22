using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Checklists.Update;

public sealed record UpdateChecklistCommand(
    Guid Id,
    bool isActive,
    int version,
    DateTime lastModified,
    bool isValid,
    string title,
    ICollection<ChecklistGroupDto> groups,
    ICollection<ChecklistQuestionDto> questions
) : ICommand; 
