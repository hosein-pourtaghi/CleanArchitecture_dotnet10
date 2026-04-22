// FileStorage.Infrastructure/Persistence/Repositories/FileAccessLogRepository.cs
using FileStorage.Domain.Entities;
using FileStorage.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.Infrastructure.Persistence.Repositories;

public class FileAccessLogRepository : IFileAccessLogRepository
{
    private readonly FileStorageDbContext _context;

    public FileAccessLogRepository(FileStorageDbContext context)
    {
        _context = context;
    }

    public async Task<FileAccessLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FileAccessLogs
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FileAccessLog>> GetByFileIdAsync(
        Guid fileId,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.FileAccessLogs
            .Where(x => x.FileAttachmentId == fileId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByFileIdAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        return await _context.FileAccessLogs
            .CountAsync(x => x.FileAttachmentId == fileId, cancellationToken);
    }

    public async Task<IReadOnlyList<FileAccessLog>> GetByUserAsync(
        Guid userId,
        DateTime? from = null,
        DateTime? to = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var q = _context.FileAccessLogs.Where(x => x.UserId == userId);

        if (from.HasValue)
            q = q.Where(x => x.CreatedAt >= from);
        if (to.HasValue)
            q = q.Where(x => x.CreatedAt <= to);

        return await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FileAccessLog>> GetFailedAccessAsync(
        DateTime? from = null,
        DateTime? to = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var q = _context.FileAccessLogs.Where(x => !x.Success);

        if (from.HasValue)
            q = q.Where(x => x.CreatedAt >= from);
        if (to.HasValue)
            q = q.Where(x => x.CreatedAt <= to);

        return await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        FileAccessLog log,
        CancellationToken cancellationToken = default)
    {
        await _context.FileAccessLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<FileAccessLog> logs,
        CancellationToken cancellationToken = default)
    {
        await _context.FileAccessLogs.AddRangeAsync(logs, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
