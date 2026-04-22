// FileStorage.Domain/Interfaces/Repositories/IFileAccessLogRepository.cs
using FileStorage.Domain.Entities;
using FileStorage.Domain.Enums;

namespace FileStorage.Domain.Interfaces.Repositories;

public interface IFileAccessLogRepository
{
    Task<FileAccessLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAccessLog>> GetByFileIdAsync(Guid fileId, int skip = 0, int take = 100, CancellationToken cancellationToken = default);
    Task<int> GetCountByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAccessLog>> GetByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null, int skip = 0, int take = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileAccessLog>> GetFailedAccessAsync(DateTime? from = null, DateTime? to = null, int skip = 0, int take = 100, CancellationToken cancellationToken = default);
    Task AddAsync(FileAccessLog log, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<FileAccessLog> logs, CancellationToken cancellationToken = default);
}
