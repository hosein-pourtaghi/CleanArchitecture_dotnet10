using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly PermissionProvider _permissionProvider;

    public PermissionAuthorizationHandler(PermissionProvider permissionProvider)
    {
        _permissionProvider = permissionProvider;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User == null || !context.User.Identity?.IsAuthenticated == true)
        {
            return Task.CompletedTask;
        }

        if (requirement.Permissions == null || !requirement.Permissions.Any())
        {
            // No specific permissions required, allow if authenticated
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Uses PermissionProvider.GetPermissions() - queries from DB with cache
        if (_permissionProvider.HasAnyPermission(requirement.Permissions))
        {
            context.Succeed(requirement);
        }

        if (_permissionProvider.HasAnyPermission(requirement.Permissions))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
