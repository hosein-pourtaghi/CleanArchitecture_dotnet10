// FileStorage.Domain/Interfaces/Repositories/IFileHistoryRepository.cs
using FileStorage.Domain.Entities;

namespace FileStorage.Domain.Interfaces.Repositories;

public interface IFileHistoryRepository
{
    Task<FileHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileHistory>> GetByFileIdAsync(Guid fileId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<int> GetCountByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileHistory>> GetByOwnerAsync(Guid ownerId, string ownerType, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task AddAsync(FileHistory history, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<FileHistory> histories, CancellationToken cancellationToken = default);
}
