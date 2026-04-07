namespace Domain.Entities.Identities;

public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;        // e.g., "users.read"
    public string Description { get; set; } = string.Empty; // e.g., "Read users"
    public string Category { get; set; } = string.Empty;    // e.g., "UserManagement"

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}


public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public ApplicationRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}


public class UserSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenVersion { get; set; } = string.Empty;  // For token invalidation
    public string RefreshTokenVersion { get; set; } = string.Empty;

    public string DeviceInfo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }

    public DateTime LoginTime { get; set; }
    public DateTime LastActivityTime { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsTerminated { get; set; }
    public string? TerminationReason { get; set; }

    public ApplicationUser User { get; set; } = null!;
}


public class TokenBlacklist
{
    public Guid Id { get; set; }
    public string TokenId { get; set; } = string.Empty;  // JTI claim
    public Guid? UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
}
