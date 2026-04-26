using IdentityApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IdentityApi.Application.Interfaces;


public interface IMyIdentityDbContext
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


}
