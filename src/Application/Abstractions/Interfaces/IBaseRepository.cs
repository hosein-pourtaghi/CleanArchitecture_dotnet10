using System.Linq.Expressions;
using Application.Common.DTOs.Shared;

namespace Application.Abstractions.Interfaces;

public interface IBaseRepository<T> where T : class
{
    IQueryable<T> ApplyFiltering(IQueryable<T> query, PaginatedRequest filter);
    IQueryable<T> ApplySorting(IQueryable<T> query, PaginatedRequest filter);
    Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<PaginatedResult<TDto>> GetAllAsync<TDto>(
      PaginatedRequest filter,
      Expression<Func<T, bool>>? additionalFilter = null);

    Task<PaginatedResult<T>> GetAllAsync(
      PaginatedRequest filter,
      Expression<Func<T, bool>>? additionalFilter = null);

  
}
