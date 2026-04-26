
namespace IdentityApi.Application.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string? FirstName,
    string? LastName
);

public record LoginRequest(
    string Email,
    string Password, 
    string? DeviceInfo = null
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    DateTime RefreshTokenExpiresAt,
    UserDto User
);

public record RefreshTokenRequest(string RefreshToken);

public record LogoutRequest(bool TerminateAllSessions = false);

public record UserDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string DisplayName,
    bool IsOnline,
    DateTime? LastLoginAt,
    List<string> Roles,
    List<string> Permissions
);

public record TokenInfo(
    string AccessToken,
    string RefreshToken,
    DateTime IssuedAt,
    DateTime ExpiresAt,
    DateTime RefreshTokenExpiresAt,
    int TokenVersion,
    int RefreshTokenVersion
);

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    int Priority,
    List<PermissionDto> Permissions,
    int UserCount
);

public record CreateRoleRequest(
    string Name,
    string? Description,
    List<Guid> PermissionIds
);

public record UpdateRoleRequest(
    string? Name,
    string? Description, 
    List<Guid>? PermissionIds
);

public record PermissionDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    bool IsAssigned
);

public record SessionDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string DeviceInfo,
    string IpAddress,
    DateTime LoginTime,
    DateTime LastActivityTime,
    DateTime? ExpiresAt,
    bool IsActive
);

public record OnlineUsersResponse(
    int TotalCount,
    List<SessionDto> Sessions
);

public record TerminateSessionRequest(Guid SessionId);

public record TerminateAllSessionsRequest(Guid? UserId = null);
