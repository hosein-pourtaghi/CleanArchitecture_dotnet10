using Application.Common.DTOs.Shared;

namespace Application.Abstractions.Interfaces;

public interface IBaseRepository<T> where T : class
{
    IQueryable<T> ApplyFiltering(IQueryable<T> query, PaginatedRequest filter);
    IQueryable<T> ApplySorting(IQueryable<T> query, PaginatedRequest filter);
    Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken);
    Task<List<T>> CreateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken);

    Task<PaginatedResult<TDto>> GetAllAsync<TDto>(PaginatedRequest filter, CancellationToken cancellationToken);

    Task<PaginatedResult<T>> GetAllAsync(PaginatedRequest filter, CancellationToken cancellationToken);


}
