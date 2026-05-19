using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedKernel;

namespace Domain.Aggregates.Checklists; 

public sealed record ChecklistUpdatedDomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
    public int EventSequence { get; init; } = 1;

    // Checklist identification
    public Guid ChecklistId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int Version { get; init; }
    public int PreviousVersion { get; init; }
    public bool IsActive { get; init; }
    public bool IsValid { get; init; }
    public float TotalScore { get; init; }
    public int GroupCount { get; init; }

    // What changed
    public ChecklistChangeType ChangeType { get; init; }
    public IReadOnlyList<string> ChangedFields { get; init; } = [];

    // Audit
    public Guid? UpdatedById { get; init; }
    public string? UpdatedByName { get; init; }
    public DateTime UpdatedAtUtc { get; init; }

    // Metadata
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public Guid? CausationId { get; init; }
    public string Source { get; init; } = "ChecklistService";
    public int EventVersion { get; init; } = 1;
}

public enum ChecklistChangeType
{
    StructureChanged,
    TitleChanged,
    StatusChanged,
    GroupsReordered,
    QuestionsModified
}
