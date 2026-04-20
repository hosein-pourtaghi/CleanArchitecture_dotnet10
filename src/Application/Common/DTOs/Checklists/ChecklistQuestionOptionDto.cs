using Domain.Aggregates.Checklists;

namespace Application.Common.DTOs.Checklists;

public class ChecklistQuestionOptionDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;


    public string? Title { get; set; }
    public string? Description { get; set; }
    /// <summary>
    /// input type for answer
    /// </summary>
    public InputType Type { get; set; }
    public Guid QuestionId { get; set; }

 
}
