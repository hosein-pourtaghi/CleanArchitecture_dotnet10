using System.Linq.Expressions;
using Application.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces.Core;

/// <summary>
/// Advanced generic repository interface for aggregate roots
/// </summary>
public interface IAdvancedRepository<TEntity> where TEntity : class
{
    #region Basic CRUD
    Task<TEntity?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync<TKey>(TKey id, CancellationToken cancellationToken = default);
    #endregion

    #region Querying
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    #endregion

    #region Include/Load Related Data
    Task<TEntity?> GetByIdWithIncludesAsync<TKey>(TKey id, params Expression<Func<TEntity, object>>[] includes);
    Task<List<TEntity>> GetAllWithIncludesAsync(params Expression<Func<TEntity, object>>[] includes);
    #endregion

    #region Aggregate Update (Generic)
    /// <summary>
    /// Generic method to update aggregate root with all nested entities
    /// Automatically detects and manages EntityState for all related entities
    /// </summary>
    Task<TEntity> UpdateAggregateAsync<TKey>(
        TKey id,
        TEntity newEntity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generic method with custom configuration
    /// </summary>
    Task<TEntity> UpdateAggregateAsync<TKey>(
        TKey id,
        TEntity newEntity,
        AggregateConfiguration<TEntity> configuration,
        CancellationToken cancellationToken = default);
    #endregion

    #region Batch Operations
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    #endregion
     
}
