using System;

namespace Application.Common.DTOs;

public class ChecklistDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsValid { get; set; }
    public string Title { get; set; }
    public ICollection<ChecklistGroupDto> Groups { get; set; }
}
