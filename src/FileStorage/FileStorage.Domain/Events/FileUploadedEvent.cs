// FileStorage.Domain/Events/FileUploadedEvent.cs
using SharedKernel;
using SharedKernel.Messaging;

namespace FileStorage.Domain.Events;

public class FileUploadedEvent : IDomainEvent
{
    public Guid FileId { get; }
    public string OriginalFileName { get; }
    public string ContentType { get; }
    public long FileSize { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public FileUploadedEvent(Domain.Entities.FileAttachment file)
    {
        FileId = file.Id;
        OriginalFileName = file.OriginalFileName;
        ContentType = file.ContentType;
        FileSize = file.FileSize;
    }
}

public class FileDeletedEvent : IDomainEvent
{
    public Guid FileId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public FileDeletedEvent(Guid fileId) => FileId = fileId;
}

public class FileRestoredEvent : IDomainEvent
{
    public Guid FileId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public FileRestoredEvent(Guid fileId) => FileId = fileId;
}

public class FileMetadataUpdatedEvent : IDomainEvent
{
    public Guid FileId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public FileMetadataUpdatedEvent(Guid fileId) => FileId = fileId;
}

public class FileAccessLevelChangedEvent : IDomainEvent
{
    public Guid FileId { get; }
    public Enums.AccessLevel OldLevel { get; }
    public Enums.AccessLevel NewLevel { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public FileAccessLevelChangedEvent(Guid fileId, Enums.AccessLevel oldLevel, Enums.AccessLevel newLevel)
    {
        FileId = fileId;
        OldLevel = oldLevel;
        NewLevel = newLevel;
    }
}

public class FileOwnershipTransferredEvent : IDomainEvent
{
    public Guid FileId { get; }
    public Guid? NewOwnerId { get; }
    public string? NewOwnerType { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public FileOwnershipTransferredEvent(Guid fileId, Guid? newOwnerId, string? newOwnerType)
    {
        FileId = fileId;
        NewOwnerId = newOwnerId;
        NewOwnerType = newOwnerType;
    }
}
