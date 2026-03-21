using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedKernel;

namespace Domain.Checklists;

public class AssessmentAnswer : Entity
{
    public Guid Id { get; set; }
    public Guid AssessmentId { get; set; }
    public Guid QuestionId { get; set; } // References question at assessment time
    public string? AnswerText { get; set; }
    public List<Guid>? SelectedOptionIds { get; set; } // For radio/checkbox

    #region Navigation
    public virtual Assessment Assessment { get; set; } = null!;
    public virtual ChecklistQuestion Question { get; set; } = null!;
    #endregion
}
