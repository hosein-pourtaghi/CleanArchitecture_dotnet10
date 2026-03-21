using SharedKernel;

namespace Domain.Checklists;

public class ChecklistGroup : Entity
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// عنوان گروه
    /// </summary>
    public string? Title { get; set; }
    /// <summary>
    /// ایدی چکلیست
    /// </summary>
    public Guid ChecklistId { get; set; }
    /// <summary>
    /// ایدی پدر
    /// </summary>
    public Guid? ParentId { get; set; }
    /// <summary>
    /// اولویت
    /// </summary>
    public int Priority { get; set; } = 1;
    /// <summary>
    /// نمایش/عدم نمایش
    /// </summary>
    public bool IsShow { get; set; } = true;

    #region Navigation
    public virtual Checklist Checklist { get; set; }
    public virtual ChecklistGroup? Parent { get; set; }
    public virtual ICollection<ChecklistQuestion> Questions { get; set; } = new HashSet<ChecklistQuestion>();
    public virtual ICollection<ChecklistGroup> Children { get; set; } = new HashSet<ChecklistGroup>();
    #endregion

    public ChecklistGroup Clone()
    {
        return new ChecklistGroup
        {
            Id = Guid.NewGuid(),
            Title = Title,
            Priority = Priority,
            IsShow = IsShow,
            ParentId = ParentId,
            ChecklistId = ChecklistId,
            // Deep clone children
            Children = Children.Select(c => c.Clone()).ToList(),
            // Deep clone questions
            Questions = Questions.Select(q => q.Clone()).ToList()
        };
    }
}
