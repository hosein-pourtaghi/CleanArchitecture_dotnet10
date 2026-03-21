namespace Application.Common.DTOs;

public class AssessmentReportItem
{
    public Guid AssessmentId { get; set; }
    public string ChecklistTitle { get; set; }
    public int ChecklistVersion { get; set; }
    public DateTime AssessmentDate { get; set; }
    public float TotalScore { get; set; }
    public int QuestionCount { get; set; }
    public int CompletedQuestionCount { get; set; }
    public float CompletionPercentage { get; set; }
}
