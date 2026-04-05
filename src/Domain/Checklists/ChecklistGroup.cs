using SharedKernel.BaseEntities;

namespace Domain.Checklists;

public class ChecklistGroup : Entity
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Title { get; set; }
    public Guid ChecklistId { get; set; }
    public Guid? ParentId { get; set; }
    public int Priority { get; set; } = 1;
    public bool IsShow { get; set; } = true;

    #region Navigation
    public virtual Checklist Checklist { get; set; }
    public virtual ChecklistGroup? Parent { get; set; }
    public virtual ICollection<ChecklistQuestion> Questions { get; set; } = new HashSet<ChecklistQuestion>();
    public virtual ICollection<ChecklistGroup> Children { get; set; } = new HashSet<ChecklistGroup>();
    #endregion

    #region Domain Methods

    /// <summary>
    /// Create new version of this group with updated data
    /// </summary>
    public ChecklistGroup CreateNewVersion(ChecklistGroup updatedTemplate)
    {
        var newId = Guid.NewGuid();

        return new ChecklistGroup
        {
            Id = newId,
            Title = updatedTemplate.Title,
            Priority = updatedTemplate.Priority,
            IsShow = true, // New version is always visible
            IsActive = updatedTemplate.IsActive,
            ParentId = this.ParentId,
            ChecklistId = this.ChecklistId,

            // Deep clone children with versioning
            Children = (updatedTemplate.Children ?? Enumerable.Empty<ChecklistGroup>())
                .Select(newChild =>
                {
                    var oldChild = this.Children.FirstOrDefault(c => c.Id == newChild.Id);
                    return oldChild?.CreateNewVersion(newChild) ?? CreateNewGroupFromTemplate(newChild, newId);
                })
                .ToList(),

            // Deep clone questions with versioning
            Questions = (updatedTemplate.Questions ?? Enumerable.Empty<ChecklistQuestion>())
                .Select(newQuestion =>
                {
                    var oldQuestion = this.Questions.FirstOrDefault(q => q.Id == newQuestion.Id);
                    return oldQuestion?.CreateNewVersion(newQuestion, newId) ?? CreateNewQuestionFromTemplate(newQuestion, newId);
                })
                .ToList()
        };
    }

    /// <summary>
    /// Create new version for CreateNewVersion of checklist
    /// </summary>
    public ChecklistGroup CreateNewVersion()
    {
        var newId = Guid.NewGuid();

        return new ChecklistGroup
        {
            Id = newId,
            Title = this.Title,
            Priority = this.Priority,
            IsShow = this.IsShow,
            IsActive = this.IsActive,
            ParentId = this.ParentId,
            ChecklistId = this.ChecklistId,
            Children = this.Children.Select(c => c.CreateNewVersion()).ToList(),
            Questions = this.Questions.Select(q => q.CreateNewVersion(newId)).ToList()
        };
    }

    private static ChecklistGroup CreateNewGroupFromTemplate(ChecklistGroup template, Guid parentId)
    {
        var newId = Guid.NewGuid();
        return new ChecklistGroup
        {
            Id = newId,
            Title = template.Title,
            Priority = template.Priority,
            IsShow = true,
            IsActive = template.IsActive,
            ParentId = parentId,
            ChecklistId = template.ChecklistId,
            Children = (template.Children ?? Enumerable.Empty<ChecklistGroup>())
                .Select(c => CreateNewGroupFromTemplate(c, newId))
                .ToList(),
            Questions = (template.Questions ?? Enumerable.Empty<ChecklistQuestion>())
                .Select(q => CreateNewQuestionFromTemplate(q, newId))
                .ToList()
        };
    }

    private static ChecklistQuestion CreateNewQuestionFromTemplate(ChecklistQuestion template, Guid groupId)
    {
        var newId = Guid.NewGuid();
        return new ChecklistQuestion
        {
            Id = newId,
            Title = template.Title,
            Score = template.Score,
            Priority = template.Priority,
            IsRequiredAnswer = template.IsRequiredAnswer,
            Type = template.Type,
            GroupId = groupId,
            Options = (template.Options ?? Enumerable.Empty<ChecklistQuestionOption>())
                .Select(o => o.CreateNewVersion(newId))
                .ToList()
        };
    }

    #endregion
}
