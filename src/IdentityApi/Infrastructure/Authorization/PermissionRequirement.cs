using Microsoft.AspNetCore.Authorization;

namespace IdentityApi.Infrastructure.Authorization;


public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string[] Permissions { get; }

    public PermissionRequirement(params string[] permissions)
    {
        Permissions = permissions ?? Array.Empty<string>();
    }
}
