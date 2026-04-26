using IdentityApi.Application.Interfaces;
using IdentityApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
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
    //private readonly IDomainEventsDispatcher _domainEventsDispatcher;

    public MyIdentityDbContext(
        DbContextOptions<MyIdentityDbContext> options,
        ICurrentUserService currentUserService
    //,
    //IDomainEventsDispatcher domainEventsDispatcher
    ) : base(options)
    {

        _currentUserService = currentUserService;
        //_domainEventsDispatcher = domainEventsDispatcher;

        // 🔥 PERFORMANCE: Disable auto-detect changes for bulk operations
        //ChangeTracker.AutoDetectChangesEnabled = false;

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
        // it should be placed here, otherwise it will rewrite the following settings!
        base.OnModelCreating(modelBuilder);

        // 🔥 SCHEMA
        modelBuilder.HasDefaultSchema(Schemas.Identity);

        // 🔥 CONFIGURE IDENTITY ENTITIES
        ConfigureIdentityEntities(modelBuilder);

        // 🔥 APPLY CONFIGURATIONS
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyIdentityDbContext).Assembly);

        // optional: additional configuration can be placed here

    }


    #region 🔥 IDENTITY CONFIGURATION

    private static void ConfigureIdentityEntities(ModelBuilder modelBuilder)
    {
        // 🔥 Configure ApplicationUser
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users", Schemas.Identity);

            // Indexes for performance
            entity.HasIndex(e => e.NormalizedEmail).HasDatabaseName("IX_Users_Email");
            entity.HasIndex(e => e.NormalizedUserName).HasDatabaseName("IX_Users_UserName").IsUnique();
        });

        // 🔥 Configure ApplicationRole
        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles", Schemas.Identity);
            entity.HasIndex(e => e.NormalizedName).HasDatabaseName("IX_Roles_Name").IsUnique();
        });

        // 🔥 Configure IdentityUserLogin
        modelBuilder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins", Schemas.Identity);
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
        });

        // 🔥 Configure IdentityUserToken
        modelBuilder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens", Schemas.Identity);
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
        });

        // 🔥 Configure IdentityRoleClaim
        modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("RoleClaims", Schemas.Identity);
            entity.HasIndex(e => e.RoleId);
        });

        // 🔥 Configure IdentityUserClaim
        modelBuilder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("UserClaims", Schemas.Identity);
            entity.HasIndex(e => e.UserId);
        });

        // 🔥 Configure IdentityUserRole
        modelBuilder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("UserRoles", Schemas.Identity);
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasIndex(e => e.RoleId);
        });
    }

    #endregion


    #region SAVE CHANGES - AUDIT & DOMAIN EVENTS

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

    #endregion




}
