using SharedKernel;

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
    /// Standard Clone (Used for CreateNewVersion)
    /// </summary>
    /// <returns></returns>
    public ChecklistGroup Clone()
    {
        return new ChecklistGroup
        {
            // 1. Generate a NEW ID for this version
            Id = Guid.NewGuid(),

            // 2. Copy values from the Client's Input (updatedTemplate)
            Title = this.Title,
            Priority = this.Priority,
            IsShow = this.IsShow,
            IsActive = this.IsActive,

            // 3. Keep the OLD relationships
            ParentId = this.ParentId,
            ChecklistId = this.ChecklistId,

            // 4. Deep clone children (Recursive)
            // We need to match children by ID to update them correctly
            Children = this.Children.Select(newChild =>
            {
                var oldChild = this.Children.FirstOrDefault(c => c.Id == newChild.Id);
                return oldChild?.Clone() ?? newChild; // Clone if exists, else add new
            }).ToList(),

            // 5. Deep clone questions
            Questions = this.Questions.Select(newQuestion =>
            {
                var oldQuestion = this.Questions.FirstOrDefault(q => q.Id == newQuestion.Id);
                return oldQuestion?.Clone() ?? newQuestion; // Clone if exists, else add new
            }).ToList()
        };


        //var newGroup = new ChecklistGroup
        //{
        //    Id = Guid.NewGuid(),
        //    Title = Title,
        //    Priority = Priority,
        //    IsShow = IsShow,
        //    ParentId = ParentId,
        //    ChecklistId = this.ChecklistId
        //};

        //if (Children != null)
        //{
        //    foreach (var child in Children)
        //    {
        //        var clonedChild = child.Clone();
        //        clonedChild.ParentId = newGroup.Id;
        //        newGroup.Children.Add(clonedChild);
        //    }
        //}

        //if (Questions != null)
        //{
        //    foreach (var question in Questions)
        //    {
        //        var clonedQuestion = question.Clone();
        //        clonedQuestion.GroupId = newGroup.Id;
        //        newGroup.Questions.Add(clonedQuestion);
        //    }
        //}

        //return newGroup;

    }
    /// <summary>
    /// Update Clone (Used for UpdateStructure)
    /// </summary>
    /// <param name="updatedTemplate"></param>
    /// <returns></returns>
    public ChecklistGroup Clone(ChecklistGroup updatedTemplate)
    {
        return new ChecklistGroup
        {
            Id = Guid.NewGuid(),
            Title = updatedTemplate.Title,
            Priority = updatedTemplate.Priority,
            IsShow = updatedTemplate.IsShow,
            IsActive = updatedTemplate.IsActive,
            ParentId = this.ParentId,
            ChecklistId = this.ChecklistId,

            // Deep clone children
            Children = updatedTemplate.Children.Select(newChild =>
            {
                var oldChild = this.Children.FirstOrDefault(c => c.Id == newChild.Id);
                return oldChild?.Clone(newChild) ?? newChild;
            }).ToList(),

            // Deep clone questions
            Questions = updatedTemplate.Questions.Select(newQuestion =>
            {
                var oldQuestion = this.Questions.FirstOrDefault(q => q.Id == newQuestion.Id);
                return oldQuestion?.Clone(newQuestion) ?? newQuestion;
            }).ToList()
        };
    }
    #endregion

}
