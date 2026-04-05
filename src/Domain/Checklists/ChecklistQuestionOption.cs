using SharedKernel.BaseEntities;

namespace Domain.Checklists;

public class ChecklistQuestionOption : Entity
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public InputType Type { get; set; }
    public Guid QuestionId { get; set; }

    #region Navigation
    public virtual ChecklistQuestion ChecklistQuestion { get; set; }
    #endregion

    #region Domain Methods

    /// <summary>
    /// Create new version with updated data
    /// </summary>
    public ChecklistQuestionOption CreateNewVersion(ChecklistQuestionOption updatedTemplate, Guid questionId)
    {
        return new ChecklistQuestionOption
        {
            Id = Guid.NewGuid(),
            Title = updatedTemplate.Title,
            Description = updatedTemplate.Description,
            Type = updatedTemplate.Type,
            IsActive = updatedTemplate.IsActive,
            QuestionId = questionId
        };
    }

    /// <summary>
    /// Create new version for checklist versioning
    /// </summary>
    public ChecklistQuestionOption CreateNewVersion(Guid questionId)
    {
        return new ChecklistQuestionOption
        {
            Id = Guid.NewGuid(),
            Title = this.Title,
            Description = this.Description,
            Type = this.Type,
            IsActive = this.IsActive,
            QuestionId = questionId
        };
    }

    #endregion
}
