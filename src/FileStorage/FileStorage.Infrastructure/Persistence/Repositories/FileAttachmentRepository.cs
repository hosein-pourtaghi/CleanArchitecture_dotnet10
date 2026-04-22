// FileStorage.Infrastructure/Persistence/Repositories/FileAttachmentRepository.cs
using FileStorage.Domain.Entities;
using FileStorage.Domain.Enums;
using FileStorage.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.Infrastructure.Persistence.Repositories;

public class FileAttachmentRepository : IFileAttachmentRepository
{
    private readonly FileStorageDbContext _context;

    public FileAttachmentRepository(FileStorageDbContext context)
    {
        _context = context;
    }

    public async Task<FileAttachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FileAttachments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<FileAttachment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FileAttachments
            .Include(x => x.History)
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FileAttachment>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await _context.FileAttachments
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FileAttachment>> GetByOwnerAsync(
        Guid ownerId,
        string ownerType,
        CancellationToken cancellationToken = default)
    {
        return await _context.FileAttachments
            .Where(x => x.OwnerId == ownerId && x.OwnerType == ownerType && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FileAttachment>> GetByOwnerPropertyAsync(
        Guid ownerId,
        string ownerType,
        string ownerProperty,
        CancellationToken cancellationToken = default)
    {
        return await _context.FileAttachments
            .Where(x => x.OwnerId == ownerId &&
                        x.OwnerType == ownerType &&
                        x.OwnerProperty == ownerProperty &&
                        !x.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FileAttachment>> GetDeletedAsync(
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.FileAttachments
            .Where(x => x.IsDeleted && x.Bucket == FileBucket.Trash)
            .OrderByDescending(x => x.DeletedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FileAttachment>> GetAbandonedAsync(
        AbandonedFilesQuery query,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-query.OrphanedDays);

        var q = _context.FileAttachments
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (query.IncludeNoOwner)
        {
            q = q.Where(x => x.OwnerId == null || x.OwnerType == null);
        }

        if (query.IncludeOrphaned)
        {
            q = q.Where(x => x.OwnerId != null &&
                             x.OwnerType != null &&
                             x.DeletedAt != null &&
                             x.DeletedAt < cutoffDate);
        }

        return await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip(query.Skip)
            .Take(query.Take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetAbandonedCountAsync(
        AbandonedFilesQuery query,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-query.OrphanedDays);

        var q = _context.FileAttachments
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (query.IncludeNoOwner)
        {
            q = q.Where(x => x.OwnerId == null || x.OwnerType == null);
        }

        if (query.IncludeOrphaned)
        {
            q = q.Where(x => x.OwnerId != null &&
                             x.OwnerType != null &&
                             x.DeletedAt != null &&
                             x.DeletedAt < cutoffDate);
        }

        return await q.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FileAttachment>> SearchAsync(
        FileSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = BuildSearchQuery(query);

        return await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip(query.Skip)
            .Take(query.Take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> SearchCountAsync(
        FileSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = BuildSearchQuery(query);
        return await q.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FileAttachment>> GetExpiredTrashAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default)
    {
        return await _context.FileAttachments
            .Where(x => x.IsDeleted &&
                        x.Bucket == FileBucket.Trash &&
                        x.DeletedAt != null &&
                        x.DeletedAt < olderThan)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        FileAttachment file,
        CancellationToken cancellationToken = default)
    {
        await _context.FileAttachments.AddAsync(file, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        FileAttachment file,
        CancellationToken cancellationToken = default)
    {
        _context.FileAttachments.Update(file);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        FileAttachment file,
        CancellationToken cancellationToken = default)
    {
        _context.FileAttachments.Remove(file);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<FileAttachment> BuildSearchQuery(FileSearchQuery query)
    {
        var q = _context.FileAttachments.AsQueryable();

        if (!query.IncludeDeleted)
            q = q.Where(x => !x.IsDeleted);

        if (!string.IsNullOrEmpty(query.SearchTerm))
            q = q.Where(x => x.OriginalFileName.Contains(query.SearchTerm) ||
                             x.StoredFileName.Contains(query.SearchTerm));

        if (query.OwnerId.HasValue)
            q = q.Where(x => x.OwnerId == query.OwnerId);

        if (!string.IsNullOrEmpty(query.OwnerType))
            q = q.Where(x => x.OwnerType == query.OwnerType);

        if (query.Category.HasValue)
            q = q.Where(x => x.Category == query.Category);

        if (query.AccessLevel.HasValue)
            q = q.Where(x => x.AccessLevel == query.AccessLevel);

        if (query.Bucket.HasValue)
            q = q.Where(x => x.Bucket == query.Bucket);

        if (!string.IsNullOrEmpty(query.ContentType))
            q = q.Where(x => x.ContentType.Contains(query.ContentType));

        if (query.MinSize.HasValue)
            q = q.Where(x => x.FileSize >= query.MinSize);

        if (query.MaxSize.HasValue)
            q = q.Where(x => x.FileSize <= query.MaxSize);

        if (query.CreatedAfter.HasValue)
            q = q.Where(x => x.CreatedAt >= query.CreatedAfter);

        if (query.CreatedBefore.HasValue)
            q = q.Where(x => x.CreatedAt <= query.CreatedBefore);

        return q;
    }
}
