using System.Text.RegularExpressions;
using SharedKernel;

namespace Domain.Checklists;

/// <summary>
/// checklist aggregate root
/// </summary>
public class Checklist : Entity
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; private set; } = 1; // Version counter
    public DateTime LastModified { get; private set; } = DateTime.UtcNow;
    /// <summary>
    /// اعتبار سنجی چکلیست/کاربرگ انجام شده واماده استفاده است
    /// </summary>
    public bool IsValid { get; set; }

    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// بارم کلی
    /// </summary>
    public float TotalScore => Groups
       .SelectMany(g => g.Questions)
       .Sum(q => q.Score ?? 0);


    #region Navigation
    public virtual ICollection<ChecklistGroup> Groups { get; set; } = new List<ChecklistGroup>();
    /// <summary>
    /// لیست ستون های اضافی
    /// </summary>
    public virtual ICollection<ChecklistQuestion> Questions { get; set; } = new List<ChecklistQuestion>();
    #endregion


    // DOMAIN METHODS (ENFORCE AGGREGATE INTEGRITY)
    public void AddGroup(ChecklistGroup group)
    {
        if (group.ChecklistId != Id)
            throw new InvalidOperationException("Group must belong to this checklist");

        Groups.Add(group);
    }

    public void AddQuestion(ChecklistQuestion question, ChecklistGroup group)
    {
        if (!Groups.Any(g => g.Id == group.Id))
            throw new InvalidOperationException("Group not found in checklist");

        question.ChecklistGroupId = group.Id;
        group.Questions.Add(question);
    }

    // DOMAIN METHOD: Update structure with versioning
    public void UpdateStructure(Checklist newStructure)
    {
        // Only increment version if structure changed
        if (HasStructureChanged(newStructure))
        {
            Version++;
            LastModified = DateTime.UtcNow;
        }

        // Replace all groups (deep clone to prevent reference issues)
        Groups = newStructure.Groups.Select(g => g.Clone()).ToList();
    }

    // DOMAIN METHOD: Create new version for historical assessments
    public Checklist CreateNewVersion()
    {
        var newVersion = new Checklist
        {
            Id = Id,
            Title = Title,
            Version = Version + 1,
            LastModified = DateTime.UtcNow,
            IsActive = IsActive,
            IsValid = IsValid
        };

        // Deep clone groups (preserves structure at time of version)
        newVersion.Groups = Groups.Select(g => g.Clone()).ToList();
        return newVersion;
    }

    private bool HasStructureChanged(Checklist newStructure)
    {
        // Compare top-level groups
        if (Groups.Count != newStructure.Groups.Count)
            return true;

        // Check if all groups exist in new structure
        foreach (var group in Groups)
        {
            var newGroup = newStructure.Groups.FirstOrDefault(g => g.Id == group.Id);
            if (newGroup == null)
                return true; // Group removed

            // Deep check group's children and questions
            if (!HasGroupStructureChanged(group, newGroup))
                return true;
        }
        return false;
    }

    private bool HasGroupStructureChanged(ChecklistGroup oldGroup, ChecklistGroup newGroup)
    {
        // Compare group's questions
        if (oldGroup.Questions.Count != newGroup.Questions.Count)
            return true;

        // Check if all questions exist
        foreach (var question in oldGroup.Questions)
        {
            if (!newGroup.Questions.Any(q => q.Id == question.Id))
                return true;
        }

        // Compare options for each question (if applicable)
        foreach (var question in oldGroup.Questions)
        {
            var newQuestion = newGroup.Questions.FirstOrDefault(q => q.Id == question.Id);
            if (newQuestion == null)
                continue;

            if (question.Options.Count != newQuestion.Options.Count)
                return true;

            // Check option titles (simplified)
            if (!question.Options.All(o =>
                newQuestion.Options.Any(n => n.Title == o.Title)))
                return true;
        }
        return false;
    }



}


