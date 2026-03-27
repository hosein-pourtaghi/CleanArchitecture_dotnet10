using System;

namespace Application.Common.DTOs;

public class AssessmentDto
{
    public Guid Id { get; set; }
    public Guid ChecklistId { get; set; }
    public int ChecklistVersion { get; set; }
    public DateTime AssessmentDate { get; set; }
    public float TotalScore { get; set; }
    public List<AssessmentAnswerDto> Answers { get; set; }

}
