using IdentityApi.Application.Interfaces;
using IdentityApi.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Shared;

namespace IdentityApi.Infrastructure.Persistence;


public sealed class MyIdentityDbContext : IdentityDbContext<
ApplicationUser,
ApplicationRole,
Guid,
ApplicationUserClaim,
ApplicationUserRole,
ApplicationUserLogin,
ApplicationRoleClaim,
ApplicationUserToken>, IMyIdentityDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public MyIdentityDbContext(
        DbContextOptions<MyIdentityDbContext> options,
        ICurrentUserService currentUserService
    ) : base(options)
    {

        _currentUserService = currentUserService;

        // 🔥 PERFORMANCE: Enable detailed errors in development
        if (Database.IsSqlServer())
        {
            Database.SetCommandTimeout(TimeSpan.FromSeconds(60));
        }
    }


    #region Identity
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<ApplicationRole> Roles => Set<ApplicationRole>();
    public DbSet<ApplicationUserRole> UserRoles => Set<ApplicationUserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<TokenBlacklist> TokenBlacklist => Set<TokenBlacklist>();
    #endregion



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1️ Base first - Identity defaults
        base.OnModelCreating(modelBuilder);
        // 2️ Default schema for all tables
        modelBuilder.HasDefaultSchema(Schemas.Identity);
        // 3️ Custom configs override base - applied last
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyIdentityDbContext).Assembly);

    }

    #region SAVE CHANGES - AUDIT & DOMAIN EVENTS

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

    #endregion




}
