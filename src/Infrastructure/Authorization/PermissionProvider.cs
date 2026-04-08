using System.Security.Claims;
using Application.Common.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Authorization;

public class PermissionProvider(IApplicationDbContext _context, IMemoryCache _cache, IHttpContextAccessor _accessor)
{

    private const string CacheKeyPrefix = "UserPermissions_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);


    public IEnumerable<string> GetPermissions()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Enumerable.Empty<string>();

        var cacheKey = $"{CacheKeyPrefix}{userId}";

        // Query from database based on user's roles
        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            return _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList();
        }) ?? new List<string>();
    }

    public bool HasPermission(string permission)
    {
        return GetPermissions().Contains(permission);
    }

    public bool HasAnyPermission(params string[] permissions)
    {
        var userPermissions = GetPermissions();
        return permissions.Any(p => userPermissions.Contains(p));
    }

    public bool HasAllPermissions(params string[] permissions)
    {
        var userPermissions = GetPermissions();
        return permissions.All(p => userPermissions.Contains(p));
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


    #region Get Permissions from token not DB
    //public IEnumerable<string> GetPermissions()
    //{
    //    var context = _httpContextAccessor().HttpContext;
    //    if (context?.User == null)
    //        return Enumerable.Empty<string>();

    //    // Get permissions from claims
    //    return context.User
    //        .FindAll("permission")
    //        .Select(c => c.Value)
    //        .Distinct();
    //} 
    #endregion

}


