// IdentityConfiguration.cs
using IdentityApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Shared;

namespace IdentityApi.Infrastructure.Persistence.Configurations;

//public static class IdentityConfiguration
//{
//    /// <summary>
//    /// Applies all identity-related entity configurations
//    /// </summary>
//    public static void ApplyConfigurations(this ModelBuilder modelBuilder)
//    {
//        // Apply custom entity configurations
//        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
//        modelBuilder.ApplyConfiguration(new ApplicationRoleConfiguration());
//        modelBuilder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
//        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
//        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
//        modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
//        modelBuilder.ApplyConfiguration(new TokenBlacklistConfiguration());

//        // Apply ASP.NET Identity entity configurations
//        modelBuilder.ApplyConfiguration(new IdentityUserLoginConfiguration());
//        modelBuilder.ApplyConfiguration(new IdentityUserTokenConfiguration());
//        modelBuilder.ApplyConfiguration(new IdentityRoleClaimConfiguration());
//        modelBuilder.ApplyConfiguration(new IdentityUserClaimConfiguration());
//        modelBuilder.ApplyConfiguration(new IdentityUserRoleConfiguration());
//    }
//}

#region Custom Entity Configurations

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users", Schemas.Identity);

        // Indexes for performance
        builder.HasIndex(e => e.NormalizedEmail).HasDatabaseName("IX_Users_Email");
        builder.HasIndex(e => e.NormalizedUserName).HasDatabaseName("IX_Users_UserName").IsUnique();
        builder.HasIndex(e => e.IsOnline).HasDatabaseName("IX_Users_IsOnline");
    }
}

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("Roles", Schemas.Identity);
        builder.HasIndex(e => e.NormalizedName).HasDatabaseName("IX_Roles_Name").IsUnique();
    }
}

public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.ToTable("UserRoles", Schemas.Identity);

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.RoleId).HasDatabaseName("IX_UserRoles_RoleId");
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", Schemas.Identity);

        builder.HasIndex(e => e.Name).IsUnique().HasDatabaseName("IX_Permissions_Name");
        builder.HasIndex(e => e.Category).HasDatabaseName("IX_Permissions_Category");
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", Schemas.Identity);

        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions", Schemas.Identity);

        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_UserSessions_UserId");
        builder.HasIndex(e => e.TokenVersion).HasDatabaseName("IX_UserSessions_TokenVersion");
        builder.HasIndex(e => e.IsActive).HasDatabaseName("IX_UserSessions_IsActive");
        builder.HasIndex(e => e.ExpiresAt).HasDatabaseName("IX_UserSessions_ExpiresAt");

        builder.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TokenBlacklistConfiguration : IEntityTypeConfiguration<TokenBlacklist>
{
    public void Configure(EntityTypeBuilder<TokenBlacklist> builder)
    {
        builder.ToTable("TokenBlacklist", Schemas.Identity);

        builder.HasIndex(e => e.TokenId)
            .IsUnique()
            .HasDatabaseName("IX_TokenBlacklist_TokenId");

        builder.HasIndex(e => e.ExpiresAt).HasDatabaseName("IX_TokenBlacklist_ExpiresAt");
    }
}

#endregion

#region ASP.NET Identity Entity Configurations

public class IdentityUserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
    {
        builder.ToTable("UserLogins", Schemas.Identity);
        builder.HasKey(e => new { e.LoginProvider, e.ProviderKey });
    }
}

public class IdentityUserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        builder.ToTable("UserTokens", Schemas.Identity);
        builder.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
    }
}

public class IdentityRoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        builder.ToTable("RoleClaims", Schemas.Identity);
        builder.HasIndex(e => e.RoleId).HasDatabaseName("IX_RoleClaims_RoleId");
    }
}

public class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder)
    {
        builder.ToTable("UserClaims", Schemas.Identity);
        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_UserClaims_UserId");
    }
}

public class IdentityUserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
    {
        builder.ToTable("UserRoles", Schemas.Identity);
        builder.HasKey(e => new { e.UserId, e.RoleId });
        builder.HasIndex(e => e.RoleId).HasDatabaseName("IX_UserRoles_RoleId");
    }
}

#endregion
