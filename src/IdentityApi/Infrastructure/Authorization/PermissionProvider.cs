using System.Security.Claims;
using IdentityApi.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IdentityApi.Infrastructure.Authorization;


public class PermissionProvider(
    IMyIdentityDbContext _context, 
    IMemoryCache _cache, 
    IHttpContextAccessor _accessor, 
    ILogger<PermissionProvider> _logger)
{
    private const string CacheKeyPrefix = "UserPermissions_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);
    public Guid UserId => GetUserId();

    public IReadOnlyList<string> GetPermissions()
    {
        if (UserId == Guid.Empty)
        {
            _logger.LogDebug("UserId is empty, no permissions");
            return Array.Empty<string>();
        }

        var cacheKey = $"{CacheKeyPrefix}{UserId}";

        // Query from database based on user's roles 
        var permissions = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var perms = _context.UserRoles
                .Where(ur => ur.UserId == UserId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList()
                .AsReadOnly();

            _logger.LogDebug("Loaded {Count} permissions for user {UserId}", perms.Count, UserId);

            return perms;
        });

        return permissions ?? Array.Empty<string>().AsReadOnly();


    }
     
    public bool HasPermission(string permission)
    {
        var perms = GetPermissions();
        var result = perms.Contains(permission, StringComparer.OrdinalIgnoreCase);
        _logger.LogDebug("HasPermission({Permission}): {Result}", permission, result);
        return result;
    }

    public bool HasAnyPermission(params string[] permissions)
    {
        var userPermissions = GetPermissions();
        return permissions.Any(p => userPermissions.Contains(p, StringComparer.OrdinalIgnoreCase));
    }

    public bool HasAllPermissions(params string[] permissions)
    {
        var userPermissions = GetPermissions();
        return permissions.All(p => userPermissions.Contains(p, StringComparer.OrdinalIgnoreCase));
    }

    private Guid GetUserId()
    {
        var userIdClaim = _accessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
    }

    public void ClearCache(Guid userId)
    {
        _cache.Remove($"{CacheKeyPrefix}{userId}");
    }

}


