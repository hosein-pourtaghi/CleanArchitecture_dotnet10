using SharedKernel;

namespace Domain.Checklists;

public class ChecklistQuestionOption : Entity
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;


    public string? Title { get; set; }
    public string? Description { get; set; }
    /// <summary>
    /// input type for answer
    /// </summary>
    public InputType Type { get; set; }
    public Guid ChecklistQuestionId { get; set; }


    #region Navigation
    public virtual ChecklistQuestion ChecklistQuestion { get; set; }
    #endregion


    /// <summary>
    /// Standard Clone
    /// </summary>
    /// <returns></returns>
    public ChecklistQuestionOption Clone()
    {
        return new ChecklistQuestionOption
        {
            Id = Guid.NewGuid(),
            Title = Title
        };
    }

    /// <summary>
    /// Update Clone
    /// </summary>
    /// <param name="updatedTemplate"></param>
    /// <returns></returns>
    public ChecklistQuestionOption Clone(ChecklistQuestionOption updatedTemplate, Guid questionId)
    {
        return new ChecklistQuestionOption
        {
            Id = Guid.NewGuid(),
            Title = updatedTemplate.Title,
            ChecklistQuestionId = questionId
        };
    }

}
