using Application.Common.DTOs.Checklists;
using Application.Common.Messaging;

namespace  Application.Assessments.Update;

public sealed record UpdateAssessmentCommand(
    Guid Id,
    Guid checklistId,
    int checklistVersion,
    DateTime assessmentDate,
    float totalScore, 
    ICollection<AssessmentAnswerDto> answers
) : ICommand; 
