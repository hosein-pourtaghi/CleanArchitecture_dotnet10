using System;
using Domain.Entities.Assessments;
using Domain.Entities.Checklists;
using SharedKernel;

namespace Application.Common.DTOs;

public class AssessmentAnswerDto
{
    public Guid Id { get; set; }
    public Guid AssessmentId { get; set; }
    public Guid QuestionId { get; set; } // References question at assessment time
    public string? AnswerText { get; set; }
    public List<Guid>? SelectedOptionIds { get; set; } // For radio/checkbox

}
