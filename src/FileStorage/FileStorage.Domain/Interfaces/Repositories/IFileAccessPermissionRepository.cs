// FileStorage.Domain/Interfaces/Repositories/IFileAccessPermissionRepository.cs
using FileStorage.Domain.Entities;
using FileStorage.Domain.Enums;

namespace FileStorage.Domain.Interfaces.Repositories;

public interface IFileAccessPermissionRepository
{
    Task<FileAccessPermission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FileAccessPermission?> GetByFileAndUserAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAccessPermission>> GetByFileIdAsync(Guid fileId, bool includeExpired = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAccessPermission>> GetByUserIdAsync(Guid userId, bool includeExpired = false, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(FileAccessPermission permission, CancellationToken cancellationToken = default);
    Task UpdateAsync(FileAccessPermission permission, CancellationToken cancellationToken = default);
    Task DeleteAsync(FileAccessPermission permission, CancellationToken cancellationToken = default);
}
