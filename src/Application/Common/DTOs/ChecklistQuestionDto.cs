using System;
using Domain.Checklists;
using SharedKernel;

namespace Application.Common.DTOs;
 
public class ChecklistQuestionDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;


    /// <summary>
    /// سوال
    /// </summary>
    public required string Title { get; set; }
    /// <summary>
    /// ایدی گروه سوال
    /// </summary>
    public Guid ChecklistGroupId { get; set; }
    /// <summary>
    /// بارم سوال
    /// </summary>
    public float? Score { get; set; }
    /// <summary>
    /// اولویت
    /// </summary>
    public int Priority { get; set; } = 1;
    /// <summary>
    /// is answer required or not
    /// </summary>
    public bool IsRequiredAnswer { get; set; }
    /// <summary>
    /// question Type
    /// </summary>
    public ChecklistQuestionType Type { get; set; }

     
}
