using SharedKernel.BaseEntities;

namespace Domain.Entities.Checklists;

public class ChecklistQuestion : Entity
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;
    public required string Title { get; set; }
    public Guid GroupId { get; set; }
    public float? Score { get; set; }
    public int Priority { get; set; } = 1;
    public bool IsRequiredAnswer { get; set; }
    public ChecklistQuestionType Type { get; set; }

    #region Navigation
    public virtual ChecklistGroup Group { get; set; }
    public virtual ICollection<ChecklistQuestionOption>? Options { get; set; } = new HashSet<ChecklistQuestionOption>();
    #endregion

    #region Domain Methods

    /// <summary>
    /// Create new version with updated data
    /// </summary>
    public ChecklistQuestion CreateNewVersion(ChecklistQuestion updatedTemplate, Guid groupId)
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
            IsActive = updatedTemplate.IsActive,
            Options = (updatedTemplate.Options ?? Enumerable.Empty<ChecklistQuestionOption>())
                .Select(newOpt =>
                {
                    var oldOpt = this.Options?.FirstOrDefault(o => o.Id == newOpt.Id);
                    return oldOpt?.CreateNewVersion(newOpt, newId) ?? CreateNewOptionFromTemplate(newOpt, newId);
                })
                .ToList()
        };
    }

    /// <summary>
    /// Create new version for checklist versioning
    /// </summary>
    public ChecklistQuestion CreateNewVersion(Guid groupId)
    {
        var newId = Guid.NewGuid();

        return new ChecklistQuestion
        {
            Id = newId,
            Title = this.Title,
            Score = this.Score,
            Priority = this.Priority,
            IsRequiredAnswer = this.IsRequiredAnswer,
            Type = this.Type,
            GroupId = groupId,
            IsActive = this.IsActive,
            Options = this.Options?.Select(o => o.CreateNewVersion(newId)).ToList()
        };
    }

    private static ChecklistQuestionOption CreateNewOptionFromTemplate(ChecklistQuestionOption template, Guid questionId)
    {
        return new ChecklistQuestionOption
        {
            Id = Guid.NewGuid(),
            Title = template.Title,
            Description = template.Description,
            Type = template.Type,
            IsActive = template.IsActive,
            QuestionId = questionId
        };
    }

    #endregion
}
