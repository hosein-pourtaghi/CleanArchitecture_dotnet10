// FileStorage.Infrastructure/Persistence/Repositories/FileAccessPermissionRepository.cs
using FileStorage.Domain.Entities;
using FileStorage.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.Infrastructure.Persistence.Repositories;

public class FileAccessPermissionRepository : IFileAccessPermissionRepository
{
    private readonly FileStorageDbContext _context;

    public FileAccessPermissionRepository(FileStorageDbContext context)
    {
        _context = context;
    }

    public async Task<FileAccessPermission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FileAccessPermissions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<FileAccessPermission?> GetByFileAndUserAsync(
        Guid fileId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.FileAccessPermissions
            .Where(x => x.FileAttachmentId == fileId && x.UserId == userId && !x.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FileAccessPermission>> GetByFileIdAsync(
        Guid fileId,
        bool includeExpired = false,
        CancellationToken cancellationToken = default)
    {
        var q = _context.FileAccessPermissions
            .Where(x => x.FileAttachmentId == fileId && !x.IsDeleted);

        if (!includeExpired)
            q = q.Where(x => !x.ExpiresAt.HasValue || x.ExpiresAt > DateTime.UtcNow);

        return await q.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FileAccessPermission>> GetByUserIdAsync(
        Guid userId,
        bool includeExpired = false,
        CancellationToken cancellationToken = default)
    {
        var q = _context.FileAccessPermissions
            .Where(x => x.UserId == userId && !x.IsDeleted);

        if (!includeExpired)
            q = q.Where(x => !x.ExpiresAt.HasValue || x.ExpiresAt > DateTime.UtcNow);

        return await q.ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPermissionAsync(
        Guid fileId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.FileAccessPermissions
            .AnyAsync(x => x.FileAttachmentId == fileId &&
                          x.UserId == userId &&
                          !x.IsDeleted &&
                          (!x.ExpiresAt.HasValue || x.ExpiresAt > DateTime.UtcNow),
                      cancellationToken);
    }

    public async Task AddAsync(
        FileAccessPermission permission,
        CancellationToken cancellationToken = default)
    {
        await _context.FileAccessPermissions.AddAsync(permission, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        FileAccessPermission permission,
        CancellationToken cancellationToken = default)
    {
        _context.FileAccessPermissions.Update(permission);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        FileAccessPermission permission,
        CancellationToken cancellationToken = default)
    {
        _context.FileAccessPermissions.Remove(permission);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
