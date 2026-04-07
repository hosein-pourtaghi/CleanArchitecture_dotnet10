using SharedKernel.BaseEntities;

namespace Domain.Entities.Checklists;

/// <summary>
/// checklist aggregate root
/// </summary>
public class Checklist : Entity 
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; private set; } = 1;
    public DateTime LastModified { get; private set; } = DateTime.UtcNow;
    public bool IsValid { get; set; }
    public string Title { get; set; } = string.Empty;

    public float TotalScore => Groups?
        .Where(g => g.IsShow)
        .SelectMany(g => g.Questions ?? new List<ChecklistQuestion>())
        .Sum(q => q.Score ?? 0) ?? 0;

    #region Navigation
    public virtual ICollection<ChecklistGroup> Groups { get; set; } = new List<ChecklistGroup>();
    #endregion

    #region Domain Methods

    public void AddGroup(ChecklistGroup group)
    {
        group.ChecklistId = this.Id;
        Groups.Add(group);
    }

    public void AddQuestion(ChecklistQuestion question, ChecklistGroup group)
    {
        if (!Groups.Any(g => g.Id == group.Id))
            throw new InvalidOperationException("Group not found in checklist");

        question.GroupId = group.Id;
        group.Questions.Add(question);
    }

    /// <summary>
    /// Update structure with versioning - creates new versions of changed entities
    /// </summary>
    public void UpdateStructure(Checklist newStructure)
    {
        Title = newStructure.Title;
        IsActive = newStructure.IsActive;
        IsValid = newStructure.IsValid;

        var oldGroups = Groups.ToList();
        var newGroups = new List<ChecklistGroup>();
        bool structureChanged = false;

        // 1. Process Groups sent from Client
        foreach (var newGroup in newStructure.Groups.OrderBy(g => g.Priority))
        {
            var existingGroup = oldGroups.FirstOrDefault(g => g.Id == newGroup.Id);

            if (existingGroup != null)
            {
                // Group exists - check if content changed
                if (HasGroupStructureChanged(existingGroup, newGroup))
                {
                    structureChanged = true;

                    // Mark old group as hidden
                    existingGroup.IsShow = false;

                    // Create new version of group with all children
                    var versionedGroup = existingGroup.CreateNewVersion(newGroup);
                    newGroups.Add(versionedGroup);
                }
                else
                {
                    // No change, keep existing group
                    newGroups.Add(existingGroup);
                }
            }
            else
            {
                // Brand new group
                structureChanged = true;
                newGroup.ChecklistId = this.Id;
                newGroup.IsShow = true;
                newGroups.Add(newGroup);
            }
        }

        // 2. Handle Deleted Groups (exist in old but not in new)
        var deletedGroups = oldGroups
            .Where(x => newGroups.All(l => l.Id != x.Id))
            .ToList();

        if (deletedGroups.Any())
        {
            structureChanged = true;
            foreach (var group in deletedGroups)
            {
                group.IsShow = false;
            }
        }

        // 3. Apply Changes
        if (structureChanged)
        {
            Version++;
            LastModified = DateTime.UtcNow;

            // Replace groups collection content
            Groups.Clear();
            foreach (var group in newGroups.Concat(oldGroups))
            {
                Groups.Add(group);
            }
        }
    }

    /// <summary>
    /// Create new version of this checklist (for historical purposes)
    /// </summary>
    public Checklist CreateNewVersion()
    {
        var newChecklist = new Checklist
        {
            Id = Guid.NewGuid(),
            Title = Title,
            Version = Version + 1,
            LastModified = DateTime.UtcNow,
            IsActive = IsActive,
            IsValid = IsValid,
            Groups = Groups.Select(g => g.CreateNewVersion()).ToList()
        };

        foreach (var group in newChecklist.Groups)
        {
            group.ChecklistId = newChecklist.Id;
        }

        return newChecklist;
    }

    private bool HasGroupStructureChanged(ChecklistGroup oldGroup, ChecklistGroup newGroup)
    {
        // Compare Group Properties
        if (oldGroup.Title != newGroup.Title ||
            oldGroup.Priority != newGroup.Priority ||
            oldGroup.IsShow != newGroup.IsShow)
        {
            return true;
        }

        // Compare Children
        var oldChildren = oldGroup.Children?.OrderBy(c => c.Priority).ToList() ?? new List<ChecklistGroup>();
        var newChildren = newGroup.Children?.OrderBy(c => c.Priority).ToList() ?? new List<ChecklistGroup>();

        if (oldChildren.Count != newChildren.Count)
            return true;

        for (int i = 0; i < oldChildren.Count; i++)
        {
            if (HasGroupStructureChanged(oldChildren[i], newChildren[i]))
                return true;
        }

        // Compare Questions
        var oldQuestions = oldGroup.Questions?.OrderBy(q => q.Priority).ToList() ?? new List<ChecklistQuestion>();
        var newQuestions = newGroup.Questions?.OrderBy(q => q.Priority).ToList() ?? new List<ChecklistQuestion>();

        if (oldQuestions.Count != newQuestions.Count)
            return true;

        for (int i = 0; i < oldQuestions.Count; i++)
        {
            if (HasQuestionChanged(oldQuestions[i], newQuestions[i]))
                return true;
        }

        return false;
    }

    private bool HasQuestionChanged(ChecklistQuestion oldQ, ChecklistQuestion newQ)
    {
        if (oldQ.Title != newQ.Title ||
            oldQ.Score != newQ.Score ||
            oldQ.Type != newQ.Type ||
            oldQ.IsRequiredAnswer != newQ.IsRequiredAnswer)
        {
            return true;
        }

        // Compare Options
        var oldOptions = oldQ.Options?.OrderBy(o => o.Title).ToList() ?? new List<ChecklistQuestionOption>();
        var newOptions = newQ.Options?.OrderBy(o => o.Title).ToList() ?? new List<ChecklistQuestionOption>();

        if (oldOptions.Count != newOptions.Count)
            return true;

        for (int j = 0; j < oldOptions.Count; j++)
        {
            if (oldOptions[j].Title != newOptions[j].Title)
                return true;
        }

        return false;
    }

    #endregion
}
