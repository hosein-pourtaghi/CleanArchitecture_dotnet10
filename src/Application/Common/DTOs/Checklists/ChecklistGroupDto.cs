namespace Application.Common.DTOs.Checklists;


public class ChecklistGroupDto
{
    public Guid? Id { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// عنوان گروه
    /// </summary>
    public string? Title { get; set; }
    /// <summary>
    /// ایدی چکلیست
    /// </summary>
    public Guid? ChecklistId { get; set; }
    /// <summary>
    /// ایدی پدر
    /// </summary>
    public Guid? ParentId { get; set; }
    /// <summary>
    /// اولویت
    /// </summary>
    public int Priority { get; set; } = 1;
    /// <summary>
    /// نمایش/عدم نمایش
    /// </summary>
    public bool IsShow { get; set; } = true;
    public List<ChecklistQuestionDto> Questions { get; set; } = new List<ChecklistQuestionDto>();
    public List<ChecklistGroupDto> Children { get; set; } = new List<ChecklistGroupDto>();

}
