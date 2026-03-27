using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Assessments.Update;

public sealed record UpdateAssessmentCommand(
    Guid Id,
    Guid checklistId,
    int checklistVersion,
    DateTime assessmentDate,
    float totalScore, 
    ICollection<AssessmentAnswerDto> answers
) : ICommand; 
