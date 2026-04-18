using System.Runtime.CompilerServices;
using Application.Common.DTOs.Shared;

namespace Application.Common.Interfaces.Core;

public interface IBaseRepository<T> where T : class
{
    IQueryable<T> ApplyFiltering(IQueryable<T> query, PaginatedRequest filter);
    IQueryable<T> ApplySorting(IQueryable<T> query, PaginatedRequest filter);
    Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<List<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task<PaginatedResult<TDto>> GetAllAsync<TDto>(PaginatedRequest filter, CancellationToken cancellationToken = default);

    Task<PaginatedResult<T>> GetAllAsync(PaginatedRequest filter, CancellationToken cancellationToken = default);

    IAsyncEnumerable<T> StreamAllAsync(PaginatedRequest filter, [EnumeratorCancellation] CancellationToken cancellationToken);
    IAsyncEnumerable<TDto> StreamAllAsync<TDto>(PaginatedRequest filter, [EnumeratorCancellation] CancellationToken cancellationToken);

}
