using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Checklists;
using SharedKernel;

namespace Domain.Assessments;

public class Assessment : Entity
{
    public Guid Id { get; set; }
    public Guid ChecklistId { get; set; }
    public int ChecklistVersion { get; set; } // Version at assessment time
    public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
    public float TotalScore { get; set; }

    #region Navigation
    public virtual Checklist Checklist { get; set; } = null!;
    public virtual ICollection<AssessmentAnswer> Answers { get; set; } = new List<AssessmentAnswer>();
    #endregion
}
