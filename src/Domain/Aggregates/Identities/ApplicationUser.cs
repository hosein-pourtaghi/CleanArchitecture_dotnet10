using Microsoft.AspNetCore.Identity;

namespace Domain.Aggregates.Identities;

public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsActive { get; set; } = true;
    public string DisplayName => $"{FirstName} {LastName}".Trim();

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Token versioning for invalidation
    public int TokenVersion { get; set; } = 1;
    public int RefreshTokenVersion { get; set; } = 1;

    // Multi-login management
    public bool AllowMultipleSessions { get; set; } = true;
    public int MaxConcurrentSessions { get; set; } = 5;

    // Activity tracking
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsOnline { get; set; }

    // Security
    public bool MustChangePassword { get; set; }
    public DateTime? PasswordChangedAt { get; set; }

    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
}


public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }

    public ApplicationRole(string name) : base(name) { }

    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public int Priority { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
}


public class ApplicationUserRole : IdentityUserRole<Guid>
{
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ApplicationRole Role { get; set; } = null!;
}

// Required Identity types
public class ApplicationUserClaim : IdentityUserClaim<Guid> { }
public class ApplicationUserLogin : IdentityUserLogin<Guid> { }
/// <summary>
/// this project uses <ref="RolePermission"> instead of this 
/// </summary>
public class ApplicationRoleClaim : IdentityRoleClaim<Guid> { }
public class ApplicationUserToken : IdentityUserToken<Guid> { }
