using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Assessments.Create;

public sealed record CreateAssessmentCommand(
    Guid checklistId,
    int checklistVersion,
    DateTime assessmentDate,
    float totalScore,
    ICollection<AssessmentAnswerDto> answers
) : ICommand<Guid>; 
