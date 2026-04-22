// FileStorage.Domain/Interfaces/Repositories/IFileAttachmentRepository.cs
using FileStorage.Domain.Entities;
using FileStorage.Domain.Enums;

namespace FileStorage.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for FileAttachment entities.
/// </summary>
public interface IFileAttachmentRepository
{
    Task<FileAttachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FileAttachment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAttachment>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAttachment>> GetByOwnerAsync(Guid ownerId, string ownerType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAttachment>> GetByOwnerPropertyAsync(Guid ownerId, string ownerType, string ownerProperty, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAttachment>> GetDeletedAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAttachment>> GetAbandonedAsync(AbandonedFilesQuery query, CancellationToken cancellationToken = default);
    Task<int> GetAbandonedCountAsync(AbandonedFilesQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAttachment>> SearchAsync(FileSearchQuery query, CancellationToken cancellationToken = default);
    Task<int> SearchCountAsync(FileSearchQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAttachment>> GetExpiredTrashAsync(DateTime olderThan, CancellationToken cancellationToken = default);
    Task AddAsync(FileAttachment file, CancellationToken cancellationToken = default);
    Task UpdateAsync(FileAttachment file, CancellationToken cancellationToken = default);
    Task DeleteAsync(FileAttachment file, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query for abandoned files.
/// </summary>
public class AbandonedFilesQuery
{
    public bool IncludeNoOwner { get; set; } = true;
    public bool IncludeOrphaned { get; set; } = true;
    public int OrphanedDays { get; set; } = 30;
    public int NoAccessInDays { get; set; } = 90;
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 100;
}

/// <summary>
/// Query for file search.
/// </summary>  
public class FileSearchQuery
{
    public string? SearchTerm { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public FileCategory? Category { get; set; }
    public AccessLevel? AccessLevel { get; set; }
    public FileBucket? Bucket { get; set; }
    public string? ContentType { get; set; }
    public long? MinSize { get; set; }
    public long? MaxSize { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public bool IncludeDeleted { get; set; } = false;
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public string OrderBy { get; set; } = "CreatedAt";
    public bool OrderDescending { get; set; } = true;
}
