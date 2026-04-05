using Domain.Checklists;
using SharedKernel.BaseEntities;

namespace Domain.Assessments;

public class AssessmentAnswer : Entity
{
    public Guid Id { get; set; }
    public Guid AssessmentId { get; set; }
    public Guid QuestionId { get; set; }

    public string? AnswerText { get; set; }
    public List<Guid>? SelectedOptionIds { get; set; } // For radio/checkbox

    #region Navigation
    public virtual Assessment Assessment { get; set; }
    public virtual ChecklistQuestion Question { get; set; }
    #endregion
}
