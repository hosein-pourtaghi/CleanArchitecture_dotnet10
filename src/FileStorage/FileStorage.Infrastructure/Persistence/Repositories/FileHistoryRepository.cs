// FileStorage.Infrastructure/Persistence/Repositories/FileHistoryRepository.cs
using FileStorage.Domain.Entities;
using FileStorage.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.Infrastructure.Persistence.Repositories;

public class FileHistoryRepository : IFileHistoryRepository
{
    private readonly FileStorageDbContext _context;

    public FileHistoryRepository(FileStorageDbContext context)
    {
        _context = context;
    }

    public async Task<FileHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FileHistories
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FileHistory>> GetByFileIdAsync(
        Guid fileId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.FileHistories
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
        return await _context.FileHistories
            .CountAsync(x => x.FileAttachmentId == fileId, cancellationToken);
    }

    public async Task<IReadOnlyList<FileHistory>> GetByOwnerAsync(
        Guid ownerId,
        string ownerType,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.FileHistories
            .Where(x => x.OwnerId == ownerId && x.OwnerType == ownerType)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        FileHistory history,
        CancellationToken cancellationToken = default)
    {
        await _context.FileHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<FileHistory> histories,
        CancellationToken cancellationToken = default)
    {
        await _context.FileHistories.AddRangeAsync(histories, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
