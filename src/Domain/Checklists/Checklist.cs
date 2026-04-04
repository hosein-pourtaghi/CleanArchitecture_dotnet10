using SharedKernel;

namespace Domain.Checklists;

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
       .SelectMany(g => g.Questions)
       .Sum(q => q.Score ?? 0) ?? 0;


    #region Navigation
    public virtual ICollection<ChecklistGroup> Groups { get; set; } = new List<ChecklistGroup>();
    #endregion


    #region Domain Methods
    public void AddGroup(ChecklistGroup group)
    {
        // Ensure the group knows who its parent is
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
    /// Update structure with versioning
    /// This method increments checklist.Version and swaps Groups with new versions
    /// </summary>
    /// <param name="newStructure"></param>
    public void UpdateStructure(Checklist newStructure)
    {
        Title = newStructure.Title;
        IsActive = newStructure.IsActive;
        IsValid = newStructure.IsValid;

        var oldGroups = Groups.ToList(); // Materialize current groups
        var resultingGroups = new List<ChecklistGroup>();
        bool structureChanged = false;

        // 1. Process Groups sent from Client
        foreach (var newGroup in newStructure.Groups.OrderBy(g => g.Priority))
        {
            var existingGroup = oldGroups.FirstOrDefault(g => g.Id == newGroup.Id);

            if (existingGroup != null)
            {
                // Group exists. Check if content changed.
                if (HasGroupStructureChanged(existingGroup, newGroup))
                {
                    structureChanged = true;
                    // Create a new version of the group
                    var versionedGroup = existingGroup.Clone(newGroup, newGroup.ParentId);
                    resultingGroups.Add(versionedGroup);
                }
                else
                {
                    // No change, keep the existing group
                    resultingGroups.Add(existingGroup);
                }
            }
            else
            {
                // Brand new group
                structureChanged = true;
                newGroup.ChecklistId = this.Id;
                resultingGroups.Add(newGroup);
            }

        }

        // 2. Handle Deleted Groups
        var deletedGroups = oldGroups.Where(x => resultingGroups.All(l => l.Id != x.Id)).ToList();
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
            //Groups = resultingGroups.Concat(oldGroups).ToList();
           
            // به جای تخصیص مستقیم، ابتدا collection را پاک کنید و سپس اضافه کنید
            Groups.Clear();
            foreach (var group in resultingGroups)
            {
                Groups.Add(group);
            }
            foreach (var oldGroup in oldGroups)
            {
                Groups.Add(oldGroup);
            }


        }
    }

    // DOMAIN METHOD: Create new version for historical assessments
    public Checklist CreateNewVersion()
    {
        var newVersion = new Checklist
        {
            // Generate a NEW ID. You cannot have two rows with the same PK.
            Id = Guid.NewGuid(),
            Title = Title,
            Version = Version + 1,
            LastModified = DateTime.UtcNow,
            IsActive = IsActive,
            IsValid = IsValid
        };

        // Deep clone groups (preserves structure at time of version)
        newVersion.Groups = Groups.Select(g => g.Clone()).ToList();

        //// Important: Update the ChecklistId in the cloned groups to point to the NEW version
        foreach (var group in newVersion.Groups)
        {
            group.ChecklistId = newVersion.Id;
        }

        return newVersion;
    }

    private bool HasStructureChanged(Checklist newStructure)
    {
        var oldGroups = Groups ?? new List<ChecklistGroup>();
        var newGroups = newStructure.Groups ?? new List<ChecklistGroup>();

        // 1. Check if count changed
        if (oldGroups.Count != newGroups.Count)
            return true;

        // 2. Sort by Priority to ensure we compare the first item with the first item, etc.
        var sortedOld = oldGroups.OrderBy(g => g.Priority).ToList();
        var sortedNew = newGroups.OrderBy(g => g.Priority).ToList();

        for (int i = 0; i < sortedOld.Count; i++)
        {
            // 3. Deep compare each group
            if (HasGroupStructureChanged(sortedOld[i], sortedNew[i]))
                return true;
        }

        return false;
    }

    private static bool HasGroupStructureChanged(ChecklistGroup oldGroup, ChecklistGroup newGroup)
    {
        // 1. Compare Group Properties (Title, Priority, IsShow)
        if (oldGroup.Title != newGroup.Title ||
            oldGroup.Priority != newGroup.Priority ||
            oldGroup.IsShow != newGroup.IsShow)
        {
            return true;
        }

        // 2. Compare Children (Recursive)
        // Since the frontend sends the same IDs, we can match by ID or just by Index/Order.
        // Matching by Index is safer here to detect if a child was moved/removed.
        var oldChildren = oldGroup.Children?.OrderBy(c => c.Priority).ToList() ?? new List<ChecklistGroup>();
        var newChildren = newGroup.Children?.OrderBy(c => c.Priority).ToList() ?? new List<ChecklistGroup>();

        if (oldChildren.Count != newChildren.Count)
            return true;

        for (int i = 0; i < oldChildren.Count; i++)
        {
            if (HasGroupStructureChanged(oldChildren[i], newChildren[i]))
                return true;
        }

        // 3. Compare Questions
        var oldQuestions = oldGroup.Questions?.OrderBy(q => q.Priority).ToList() ?? new List<ChecklistQuestion>();
        var newQuestions = newGroup.Questions?.OrderBy(q => q.Priority).ToList() ?? new List<ChecklistQuestion>();

        if (oldQuestions.Count != newQuestions.Count)
            return true;

        for (int i = 0; i < oldQuestions.Count; i++)
        {
            var oldQ = oldQuestions[i];
            var newQ = newQuestions[i];

            // Compare Question Properties
            if (oldQ.Title != newQ.Title ||
                oldQ.Score != newQ.Score ||
                oldQ.Type != newQ.Type ||
                oldQ.IsRequiredAnswer != newQ.IsRequiredAnswer)
            {
                return true;
            }

            // 4. Compare Options
            var oldOptions = oldQ.Options?.OrderBy(o => o.Title).ToList() ?? new List<ChecklistQuestionOption>();
            var newOptions = newQ.Options?.OrderBy(o => o.Title).ToList() ?? new List<ChecklistQuestionOption>();

            if (oldOptions.Count != newOptions.Count)
                return true;

            for (int j = 0; j < oldOptions.Count; j++)
            {
                if (oldOptions[j].Title != newOptions[j].Title)
                    return true;
            }
        }

        return false;
    }
    #endregion


}


