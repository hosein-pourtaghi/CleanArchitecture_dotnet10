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
    public ChecklistQuestion ChecklistQuestion { get; set; }
    #endregion


    public ChecklistQuestionOption Clone()
    {
        return new ChecklistQuestionOption
        {
            Id = Guid.NewGuid(),
            Title = Title,
            Description = Description,
            IsActive = IsActive
        };
    }

}
