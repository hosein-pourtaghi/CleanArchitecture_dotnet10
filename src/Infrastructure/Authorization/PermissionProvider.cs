using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authorization;

public class PermissionProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionProvider(HttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IEnumerable<string> GetPermissions()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User == null)
            return Enumerable.Empty<string>();

        // Get permissions from claims
        return context.User
            .FindAll("permission")
            .Select(c => c.Value)
            .Distinct();
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
}
 

