using Application.Common.DTOs;
using Application.Common.DTOs.Identities;
using SharedKernel;

namespace Application.Common.Interfaces;
 

public interface IAuthService
{
    Task<Result<LoginResponse>> RegisterAsync(RegisterRequest request, string ipAddress, string? userAgent);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress, string? userAgent);
    Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<Result> LogoutAsync(Guid userId, LogoutRequest request, string? tokenId);
    Task<Result> ValidateTokenAsync(string token);
}

public interface ISessionService
{
    Task<Result<OnlineUsersResponse>> GetOnlineUsersAsync(int page = 1, int pageSize = 20);
    Task<Result<List<SessionDto>>> GetUserSessionsAsync(Guid userId);
    Task<Result> TerminateSessionAsync(Guid userId, Guid sessionId);
    Task<Result> TerminateAllSessionsAsync(Guid userId, Guid? exceptSessionId = null);
    Task<Result> TerminateExpiredSessionsAsync();
    Task UpdateSessionActivityAsync(Guid sessionId);
}
 
public interface IRolePermissionService
{
    // Permissions
    Task<Result<List<PermissionDto>>> GetAllPermissionsAsync();
    Task<Result<PermissionDto>> GetPermissionByIdAsync(Guid id);
    Task<Result<PermissionDto>> CreatePermissionAsync(string name, string description, string category);
    Task<Result<PermissionDto>> UpdatePermissionAsync(Guid id, string name, string description, string category);
    Task<Result> DeletePermissionAsync(Guid id);

    // Roles
    Task<Result<List<RoleDto>>> GetAllRolesAsync();
    Task<Result<RoleDto>> GetRoleByIdAsync(Guid id);
    Task<Result<RoleDto>> CreateRoleAsync(CreateRoleRequest request);
    Task<Result<RoleDto>> UpdateRoleAsync(Guid id, UpdateRoleRequest request);
    Task<Result> DeleteRoleAsync(Guid id);

    // Role-Permission Management
    Task<Result> AssignPermissionsToRoleAsync(Guid roleId, List<Guid> permissionIds);
    Task<Result> RemovePermissionsFromRoleAsync(Guid roleId, List<Guid> permissionIds);
    Task<Result> SetRolePermissionsAsync(Guid roleId, List<Guid> permissionIds);

    // User-Role Management
    Task<Result> AssignRoleToUserAsync(Guid userId, Guid roleId);
    Task<Result> RemoveRoleFromUserAsync(Guid userId, Guid roleId);
    Task<Result<List<RoleDto>>> GetUserRolesAsync(Guid userId);
    Task<Result<List<PermissionDto>>> GetUserPermissionsAsync(Guid userId);
}
 
public interface ITokenService
{
    TokenInfo GenerateTokens(Guid userId, int tokenVersion, int refreshTokenVersion, List<string> permissions, List<string> roles);
    (Guid? UserId, string? TokenId, int? TokenVersion) ValidateAccessToken(string token);
    (Guid? UserId, string? TokenId, int? RefreshTokenVersion) ValidateRefreshToken(string refreshToken);
    Task<Result> InvalidateTokenAsync(string tokenId, Guid? userId, string? reason = null);
    Task<bool> IsTokenBlacklistedAsync(string tokenId);
    Task<Result<TokenInfo>> ExtendTokenAsync(string refreshToken);
}
