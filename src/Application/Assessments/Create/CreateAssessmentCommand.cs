using Application.Common.DTOs;
using Application.Common.Messaging;

namespace  Application.Assessments.Create;

public sealed record CreateAssessmentCommand(
    Guid checklistId,
    int checklistVersion,
    DateTime assessmentDate,
    float totalScore,
    ICollection<AssessmentAnswerDto> answers
) : ICommand<Guid>; 
