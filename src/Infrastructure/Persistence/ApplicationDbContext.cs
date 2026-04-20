using System.Data;
using System.Linq.Expressions;
using Application.Common.Interfaces.Core;
using Domain.Aggregates.Assessments;
using Domain.Aggregates.Checklists;
using Domain.Aggregates.Identities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using SharedKernel;
using SharedKernel.BaseEntities;

namespace Infrastructure.Persistence;


//public sealed class ApplicationDbContext : IdentityDbContext<
//    ApplicationUser,
//    ApplicationRole,
//    Guid,
//    ApplicationUserClaim,
//    ApplicationUserRole,
//    ApplicationUserLogin,
//    ApplicationRoleClaim,
//    ApplicationUserToken>


//public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
public sealed class ApplicationDbContext : IdentityDbContext<
ApplicationUser,
ApplicationRole,
Guid,
ApplicationUserClaim,
ApplicationUserRole,
ApplicationUserLogin,
ApplicationRoleClaim,
ApplicationUserToken>, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        IDomainEventsDispatcher domainEventsDispatcher
    ) : base(options)
    {

        _currentUserService = currentUserService;
        _domainEventsDispatcher = domainEventsDispatcher;

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


    #region DbSets  

    public DbSet<Checklist> Checklists => Set<Checklist>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    #endregion



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // it should be placed here, otherwise it will rewrite the following settings!
        base.OnModelCreating(modelBuilder);

        // 🔥 SCHEMA
        modelBuilder.HasDefaultSchema(Schemas.Default);

        // 🔥 CONFIGURE IDENTITY ENTITIES
        ConfigureIdentityEntities(modelBuilder);

        // 🔥 AUTO-CONFIGURE: All entities automatically
        ConfigureAllEntitiesAutomatically(modelBuilder);

        // 🔥 APPLY CONFIGURATIONS
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // optional: additional configuration can be placed here

    }


    #region 🔥 IDENTITY CONFIGURATION

    private static void ConfigureIdentityEntities(ModelBuilder modelBuilder)
    {
        // 🔥 Configure ApplicationUser
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users", Schemas.Default);

            // Indexes for performance
            entity.HasIndex(e => e.NormalizedEmail).HasDatabaseName("IX_Users_Email");
            entity.HasIndex(e => e.NormalizedUserName).HasDatabaseName("IX_Users_UserName").IsUnique();
        });

        // 🔥 Configure ApplicationRole
        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles", Schemas.Default);
            entity.HasIndex(e => e.NormalizedName).HasDatabaseName("IX_Roles_Name").IsUnique();
        });

        // 🔥 Configure IdentityUserLogin
        modelBuilder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins", Schemas.Default);
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
        });

        // 🔥 Configure IdentityUserToken
        modelBuilder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens", Schemas.Default);
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
        });

        // 🔥 Configure IdentityRoleClaim
        modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("RoleClaims", Schemas.Default);
            entity.HasIndex(e => e.RoleId);
        });

        // 🔥 Configure IdentityUserClaim
        modelBuilder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("UserClaims", Schemas.Default);
            entity.HasIndex(e => e.UserId);
        });

        // 🔥 Configure IdentityUserRole
        modelBuilder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("UserRoles", Schemas.Default);
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasIndex(e => e.RoleId);
        });
    }

    #endregion


    #region 🔥 AUTOMATIC CONFIGURATION - NO MANUAL WORK NEEDED!

    private static void ConfigureAllEntitiesAutomatically(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => IsEntityTypeValid(e.ClrType))
            .ToList();

        foreach (var entityType in entityTypes)
        {
            if (!typeof(Entity).IsAssignableFrom(entityType.ClrType))
                return;

            var entityBuilder = modelBuilder.Entity(entityType.ClrType);

            // 1. Configure Shadow Properties (Readonly + Auto-managed)
            ConfigureShadowProperties(entityBuilder, entityType.ClrType);

            // 2. Configure Soft Delete + Global Query Filter
            ConfigureSoftDelete(entityBuilder, entityType.ClrType);

            // 3. Configure High-Performance Indexes
            ConfigurePerformanceIndexes(entityBuilder, entityType);
        }
    }

    private static bool IsEntityTypeValid(Type entityType)
    {
        return entityType != typeof(Entity) &&
               entityType != typeof(Entity) &&
               typeof(Entity).IsAssignableFrom(entityType);
    }

    private static void ConfigureShadowProperties(EntityTypeBuilder entity, Type entityType)
    {
        // 🔥 SHADOW PROPERTIES: CreatedAt
        entity.Property<DateTime>("CreatedAt")
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd()
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime2(3)")
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw); // READONLY!

        // 🔥 SHADOW PROPERTIES: UpdatedAt
        entity.Property<DateTime?>("UpdatedAt")
            .HasColumnName("UpdatedAt")
            .HasColumnType("datetime2(3)")
            .ValueGeneratedOnAddOrUpdate()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw); // READONLY!

        // 🔥 SHADOW PROPERTIES: CreatedById
        entity.Property<Guid?>("CreatedById")
            .HasMaxLength(450)
            .HasColumnName("CreatedById")
            .HasColumnType("uniqueidentifier")
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw); // READONLY!

        // 🔥 SHADOW PROPERTIES: UpdatedById
        entity.Property<Guid?>("UpdatedById")
            .HasMaxLength(450)
            .HasColumnName("UpdatedById")
            .HasColumnType("uniqueidentifier")
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw); // READONLY!

        // 🔥 SHADOW PROPERTIES: Only for Entity - in case i want to separate soft delete from audit
        if (typeof(Entity).IsAssignableFrom(entityType))
        {
            entity.Property<bool>("IsDeleted")
                .HasDefaultValue(false)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw); // READONLY!

            entity.Property<DateTime?>("DeletedAt")
                .HasColumnName("DeletedAt")
                .HasColumnType("datetime2(3)")
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

            entity.Property<Guid?>("DeletedById")
                .HasMaxLength(450)
                .HasColumnName("DeletedById")
                .HasColumnType("uniqueidentifier")
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        }
    }

    private static void ConfigureSoftDelete(EntityTypeBuilder entity, Type entityType)
    {
        if (!typeof(Entity).IsAssignableFrom(entityType))
            return;

        // 🔥 FIX: Use explicit type parameter for query filter
        var parameter = Expression.Parameter(entityType, "x");
        var property = Expression.Property(parameter, "IsDeleted");
        var falseConstant = Expression.Constant(false);
        var comparison = Expression.Equal(property, falseConstant);
        var lambda = Expression.Lambda(comparison, parameter);

        entity.HasQueryFilter(lambda);

        // 🔥 IGNORE SOFT DELETED: For explicit include queries
        entity.Ignore("IsDeleted");


    }

    private static void ConfigurePerformanceIndexes(EntityTypeBuilder entity, IMutableEntityType entityType)
    {
        var tableName = entityType.GetTableName();
        if (string.IsNullOrEmpty(tableName))
            return;

        // 🔥 INDEX: IsDeleted for soft delete queries (most common filter)
        entity.HasIndex("IsDeleted")
            .HasDatabaseName($"IX_{tableName}_IsDeleted")
            .IsDescending(false);

        // 🔥 INDEX: Composite indexes for common queries
        // CreatedAt for sorting/filtering
        entity.HasIndex("CreatedAt")
            .HasDatabaseName($"IX_{tableName}_CreatedAt")
            .IsDescending(false);

        // 🔥 INDEX: UpdatedAt for cache invalidation patterns
        entity.HasIndex("UpdatedAt")
            .HasDatabaseName($"IX_{tableName}_UpdatedAt")
            .IsDescending(false);
    }

    #endregion


    #region SAVE CHANGES - AUDIT & DOMAIN EVENTS

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Update audit properties BEFORE saving
        UpdateAuditProperties();

        // When should you publish domain events?
        //
        // 1. BEFORE calling SaveChangesAsync
        //     - domain events are part of the same transaction
        //     - immediate consistency
        // 2. AFTER calling SaveChangesAsync
        //     - domain events are a separate transaction
        //     - eventual consistency
        //     - handlers can fail
        // 2. Capture domain events BEFORE saving (immediate consistency)
        var domainEvents = CaptureDomainEvents();

        // 3. Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        // 4. Publish domain events AFTER saving
        if (domainEvents.Any())
        {
            await _domainEventsDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    private void UpdateAuditProperties()
    {
        var now = DateTime.UtcNow;
        var userId = _currentUserService?.UserId;

        // 🔥 AUDIT: Process added/modified entities - EFFICIENT: Process only entities that need tracking
        var auditableEntries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in auditableEntries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Set shadow properties
                    entry.Property("CreatedAt").CurrentValue = now;
                    entry.Property("CreatedById").CurrentValue = userId;

                    // Also set concrete properties for read access
                    entry.Entity.SetCreatedBy(userId);
                    break;

                case EntityState.Modified:
                    // Set shadow properties
                    entry.Property("UpdatedAt").CurrentValue = now;
                    entry.Property("UpdatedById").CurrentValue = userId;

                    // Also set concrete properties for read access
                    entry.Entity.SetUpdatedBy(userId);
                    break;
            }
        }

        // 🔥 SOFT DELETE: Handle soft delete operations
        var softDeletableEntries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in softDeletableEntries)
        {
            // Convert hard delete to soft delete
            entry.State = EntityState.Modified;
            entry.Property("IsDeleted").CurrentValue = true;
            entry.Property("DeletedAt").CurrentValue = now;
            entry.Property("DeletedById").CurrentValue = userId;

            // Also update concrete property
            entry.Entity.SoftDelete(userId);
        }
    }

    private List<IDomainEvent> CaptureDomainEvents()
    {
        return ChangeTracker.Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return events;
            })
            .ToList();
    }

    #endregion


    #region 🔥 QUERY METHODS

    public IQueryable<T> QueryWithBypassedFilters<T>() where T : class
    {
        return Set<T>().IgnoreQueryFilters();
    }

    public IQueryable<T> QueryNoTracking<T>() where T : class
    {
        return Set<T>().AsNoTracking();
    }

    public async Task<IReadOnlyList<T>> FromSqlRawAsync<T>(
        string sql,
        CancellationToken cancellationToken = default) where T : class
    {
        return await Set<T>().FromSqlRaw(sql).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> FromSqlRawAsync<T>(
        string sql,
        object[] parameters,
        CancellationToken cancellationToken = default) where T : class
    {
        return await Set<T>().FromSqlRaw(sql, parameters).ToListAsync(cancellationToken);
    }

    public async Task<int> ExecuteSqlRawAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        return await Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public async Task<int> ExecuteSqlRawAsync(
        string sql,
        object[] parameters,
        CancellationToken cancellationToken = default)
    {
        return await Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }

    #endregion


    #region 🔥 TRANSACTION MANAGEMENT

    private IDbContextTransaction? _currentTransaction;

    public async Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        _currentTransaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("No active transaction to commit.");

        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("No active transaction to rollback.");

        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    #endregion


    #region 🔥 BULK OPERATIONS (Using EF Core 8.0+)

    public async Task BulkInsertAsync<T>(
        IReadOnlyList<T> entities,
        CancellationToken cancellationToken = default) where T : class
    {
        await BulkInsertInternalAsync(entities, cancellationToken);
    }

    public async Task BulkUpdateAsync<T>(
        IReadOnlyList<T> entities,
        CancellationToken cancellationToken = default) where T : class
    {
        await BulkUpdateInternalAsync(entities, cancellationToken);
    }

    public async Task BulkDeleteAsync<T>(
        IReadOnlyList<T> entities,
        CancellationToken cancellationToken = default) where T : class
    {
        await BulkDeleteInternalAsync(entities, cancellationToken);
    }

    // 🔥 INTERNAL BULK OPERATIONS (using raw SQL for performance)
    private async Task BulkInsertInternalAsync<T>(
     IReadOnlyList<T> entities,
     CancellationToken cancellationToken) where T : class
    {
        if (!entities.Any())
            return;

        var entityType = Model.FindEntityType(typeof(T));
        if (entityType == null)
            return;

        var tableName = entityType.GetTableName();
        var schema = entityType.GetSchema();

        if (string.IsNullOrEmpty(tableName))
            return;

        // 🔥 Use EF Core model to get properties (not standard reflection!)
        var properties = entityType.GetProperties()
            .Where(p => p.IsPrimaryKey() == false)
            .ToList();

        if (!properties.Any())
            return;

        var columns = string.Join(", ", properties.Select(p => $"[{p.GetColumnName()}]"));
        var values = string.Join(", ", properties.Select((p, i) => $"@p{i}"));

        var sql = $"INSERT INTO [{schema}].[{tableName}] ({columns}) VALUES ({values})";

        foreach (var entity in entities)
        {
            var parameters = properties
                .Select(p => p.PropertyInfo?.GetValue(entity))
                .ToArray();

            await ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }
    }

    private async Task BulkUpdateInternalAsync<T>(
        IReadOnlyList<T> entities,
        CancellationToken cancellationToken) where T : class
    {
        if (!entities.Any())
            return;

        // For updates, use batched approach
        foreach (var entity in entities)
        {
            Update(entity);
        }
        await SaveChangesAsync(cancellationToken);
    }

    private async Task BulkDeleteInternalAsync<T>(
        IReadOnlyList<T> entities,
        CancellationToken cancellationToken) where T : class
    {
        if (!entities.Any())
            return;

        var entityType = Model.FindEntityType(typeof(T));
        var tableName = entityType?.GetTableName();
        var schema = entityType?.GetSchema();
        var primaryKey = entityType?.FindPrimaryKey();

        if (string.IsNullOrEmpty(tableName) || primaryKey == null)
            return;

        var pkProperty = primaryKey.Properties.First();
        var pkColumn = pkProperty.GetColumnName();

        foreach (var entity in entities)
        {
            var pkValue = pkProperty.PropertyInfo?.GetValue(entity);
            var sql = $"DELETE FROM [{schema}].[{tableName}] WHERE [{pkColumn}] = @p0";
            await ExecuteSqlRawAsync(sql, new object[] { pkValue! }, cancellationToken);
        }
    }

    #endregion



}
