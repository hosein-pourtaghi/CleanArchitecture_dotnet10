using SharedKernel;

namespace Domain.Checklists;

public class ChecklistQuestion : Entity
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;


    /// <summary>
    /// سوال
    /// </summary>
    public required string Title { get; set; }
    /// <summary>
    /// ایدی گروه سوال
    /// </summary>
    public Guid GroupId { get; set; }
    /// <summary>
    /// بارم سوال
    /// </summary>
    public float? Score { get; set; }
    /// <summary>
    /// اولویت
    /// </summary>
    public int Priority { get; set; } = 1;
    /// <summary>
    /// is answer required or not
    /// </summary>
    public bool IsRequiredAnswer { get; set; }
    /// <summary>
    /// question Type
    /// </summary>
    public ChecklistQuestionType Type { get; set; }



    #region Navigation
    public virtual ChecklistGroup Group { get; set; }
    /// <summary>
    /// list of options based on question type
    /// if question has no option assessment is a simple string
    /// </summary>
    public virtual ICollection<ChecklistQuestionOption>? Options { get; set; } = new HashSet<ChecklistQuestionOption>();
    #endregion



    /// <summary>
    /// Standard Clone
    /// </summary>
    /// <returns></returns>
    public ChecklistQuestion Clone()
    {
        return new ChecklistQuestion
        {
            Id = Guid.NewGuid(),
            Title = Title,
            Score = Score,
            Priority = Priority,
            IsRequiredAnswer = IsRequiredAnswer,
            Type = Type,
            Options = Options?.Select(o => o.Clone()).ToList()
        };
    }

    /// <summary>
    /// Update Clone
    /// </summary>
    /// <param name="updatedTemplate"></param>
    /// <returns></returns>
    public ChecklistQuestion Clone(ChecklistQuestion updatedTemplate, Guid groupId)
    {
        var newId = Guid.NewGuid();
        return new ChecklistQuestion
        {
            Id = newId,
            Title = updatedTemplate.Title,
            Score = updatedTemplate.Score,
            Priority = updatedTemplate.Priority,
            IsRequiredAnswer = updatedTemplate.IsRequiredAnswer,
            Type = updatedTemplate.Type,
            GroupId = groupId,

            // Deep clone options
            Options = updatedTemplate.Options?.Select(newOpt =>
            {
                var oldOpt = this.Options?.FirstOrDefault(o => o.Id == newOpt.Id);
                return oldOpt?.Clone(newOpt, newId) ?? newOpt;
            }).ToList()
        };
    }

}
