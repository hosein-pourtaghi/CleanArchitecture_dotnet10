using System.Data;
using Domain.Entities.Assessments;
using Domain.Entities.Checklists;
using Domain.Entities.Customers;
using Domain.Entities.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Common.Data;


public interface IApplicationDbContext
{


    #region Identity
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<ApplicationUserRole> UserRoles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserSession> UserSessions { get; }
    DbSet<TokenBlacklist> TokenBlacklist { get; }
    #endregion


    #region DbSets 
    DbSet<Customer> Customers { get; }
    DbSet<Checklist> Checklists { get; }
    DbSet<Assessment> Assessments { get; }


    // Add other DbSets here
    #endregion


    #region Db Context
    /// <summary>
    /// Database instance for raw SQL queries
    /// </summary>
    DatabaseFacade Database { get; }
    #endregion

    #region Save Changes
    /// <summary>
    /// Save all changes made in this context to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Save all changes made in this context to the database synchronously
    /// </summary>
    int SaveChanges();

    #endregion


    ////////////////////////////////////**********************       Just in case for future      *****************************

    #region Query Methods

    /// <summary>
    /// Get DbSet for entity type
    /// </summary>
    DbSet<T> Set<T>() where T : class;

    /// <summary>
    /// Execute raw SQL query
    /// </summary>
    Task<IReadOnlyList<T>> FromSqlRawAsync<T>(string sql, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Execute raw SQL query with parameters
    /// </summary>
    Task<IReadOnlyList<T>> FromSqlRawAsync<T>(string sql, object[] parameters, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Execute raw SQL non-query (INSERT, UPDATE, DELETE)
    /// </summary>
    Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute raw SQL non-query with parameters
    /// </summary>
    Task<int> ExecuteSqlRawAsync(string sql, object[] parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query with bypassed soft delete filters (for admin/background jobs)
    /// </summary>
    IQueryable<T> QueryWithBypassedFilters<T>() where T : class;

    /// <summary>
    /// Query without tracking (for read-only high-performance queries)
    /// </summary>
    IQueryable<T> QueryNoTracking<T>() where T : class;

    #endregion

    #region Transaction Management

    /// <summary>
    /// Begin a new transaction
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin a new transaction with specified isolation level
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit current transaction
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback current transaction
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Bulk insert entities (high performance)
    /// </summary>
    Task BulkInsertAsync<T>(IReadOnlyList<T> entities, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Bulk update entities (high performance)
    /// </summary>
    Task BulkUpdateAsync<T>(IReadOnlyList<T> entities, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Bulk delete entities (high performance)
    /// </summary>
    Task BulkDeleteAsync<T>(IReadOnlyList<T> entities, CancellationToken cancellationToken = default)
        where T : class;

    #endregion


}
