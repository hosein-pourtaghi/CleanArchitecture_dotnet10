using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly PermissionProvider _permissionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        PermissionProvider permissionProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _permissionProvider = permissionProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Check authentication
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("User not authenticated");
            return Task.CompletedTask;
        }

        // If specific permissions provided in requirement, check them
        if (requirement.Permissions != null && requirement.Permissions.Length > 0)
        {
            var hasPermission = _permissionProvider.HasAllPermissions(requirement.Permissions);
            _logger.LogDebug("Checking specific permissions: {Permissions}, Result: {Result}",
                string.Join(",", requirement.Permissions), hasPermission);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }

        // No specific permissions - auto-detect from route
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogDebug("No HttpContext available");
            return Task.CompletedTask;
        }

        // Extract controller and action from route
        var routeData = httpContext.Request.RouteValues;

        var controllerName = routeData["controller"]?.ToString();
        var actionName = routeData["action"]?.ToString();

        if (string.IsNullOrEmpty(controllerName))
        {
            // Allow if no route data (not a MVC controller)
            _logger.LogDebug("No controller in route, allowing");
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Generate policy name: users:getall
        var policyName = $"{controllerName.ToLower()}:{actionName?.ToLower()}";

        _logger.LogDebug("Auto-detected policy: {PolicyName}, UserId: {UserId}",
            policyName, _permissionProvider.UserId);

        // Check if user has permission for this controller:action
        if (_permissionProvider.HasPermission(policyName))
        {
            _logger.LogDebug("User has permission: {PolicyName}", policyName);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogDebug("User does NOT have permission: {PolicyName}", policyName);
            _logger.LogDebug("User permissions: {Permissions}",
                string.Join(", ", _permissionProvider.GetPermissions()));
        }

        return Task.CompletedTask;
    }
}
