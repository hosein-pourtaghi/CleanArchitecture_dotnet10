using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Domain.Aggregates.Identities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Authorization;

/// <summary>
/// find policies dynamically 
/// no need to use [Authorize(policy:"xxx")]
/// </summary>
public interface IPolicyDiscoveryService
{
    Task<Result<int>> DiscoverAndRegisterPoliciesAsync();
    List<DiscoveredPolicy> DiscoverPolicies();
}

public class DiscoveredPolicy
{
    public string PolicyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
}

public class PolicyDiscoveryService : IPolicyDiscoveryService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PolicyDiscoveryService> _logger;

    public PolicyDiscoveryService(IServiceProvider serviceProvider, ILogger<PolicyDiscoveryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public List<DiscoveredPolicy> DiscoverPolicies()
    {
        var policies = new List<DiscoveredPolicy>();

        var controllerTypes = Assembly.GetEntryAssembly()?.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        _logger.LogDebug("Found {Count} controllers in namespace",
            controllerTypes?.Count ?? 0);

        if (controllerTypes == null)
            return policies;

        foreach (var controllerType in controllerTypes)
        {
            var controllerName = controllerType.Name.Replace("Controller", "");

            // Get Area from controller
            var area = GetArea(controllerType);
            var category = string.IsNullOrEmpty(area) ? GetCategoryFromController(controllerName) : area;

            var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.DeclaringType == controllerType && !m.IsSpecialName);

            foreach (var method in methods)
            {
                var httpMethod = GetHttpMethod(method);
                var actionName = method.Name;

                // Skip non-action methods
                if (IsIgnoredAction(actionName))
                    continue;

                // Generate policy name from controller + action
                var policyName = GeneratePolicyName(controllerName, actionName);

                //// Check if already has explicit [Authorize] with policy
                //var hasExplicitPolicy = HasExplicitPolicy(method);
                //if (!hasExplicitPolicy)
                //{
                policies.Add(new DiscoveredPolicy
                {
                    PolicyName = policyName,
                    Description = actionName, // GetActionDescription(method, actionName)   // Action name as description
                    Category = category,
                    Controller = controllerName,
                    Action = actionName,
                    HttpMethod = httpMethod
                });
                //}
            }
        }
        return policies.DistinctBy(p => p.PolicyName).ToList();
    }

    public async Task<Result<int>> DiscoverAndRegisterPoliciesAsync()
    {
        try
        {
            var discoveredPolicies = DiscoverPolicies();

            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            // Get DbContext
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Get AuthorizationOptions
            var authorizationOptions = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
            var options = authorizationOptions.Value;

            var registeredCount = 0;
            var permissionsSavedCount = 0;

            foreach (var policy in discoveredPolicies)
            {
                var policyKey = $"permission:{policy.PolicyName}";

                // ✅ 1. Save permission to database if not exists
                var existingPermission = await context.Permissions
                    .FirstOrDefaultAsync(p => p.Name == policy.PolicyName);
                if (existingPermission == null)
                {
                    var newPermission = new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = policy.PolicyName,
                        Description = policy.Description,
                        Category = policy.Category,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Permissions.Add(newPermission);
                    permissionsSavedCount++;
                    _logger.LogDebug("Saved permission: {PermissionName}", policy.PolicyName);
                }

                // ✅ 2. Register policy  
                if (options.GetPolicy(policyKey) == null)
                {
                    options.AddPolicy(policyKey, builder =>
                    {
                        builder.AddRequirements(new PermissionRequirement(policy.PolicyName));
                    });
                    registeredCount++;
                }
            }

            // ✅ 3. Save all permissions to database
            await context.SaveChangesAsync();

            _logger.LogInformation("Permissions: {PermissionsSaved} saved, Policies: {PoliciesRegistered} registered",
                permissionsSavedCount, registeredCount);
            return Result.Success(registeredCount);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>($"Failed to register policies: {ex.Message}");
        }
    }

    private static string GetArea(Type controllerType)
    {
        var areaAttr = controllerType.GetCustomAttribute<AreaAttribute>();
        return areaAttr?.RouteValue ?? string.Empty;
    }

    private static string GetHttpMethod(MethodInfo method)
    {
        if (method.GetCustomAttribute<HttpGetAttribute>() != null)
            return "GET";
        if (method.GetCustomAttribute<HttpPostAttribute>() != null)
            return "POST";
        if (method.GetCustomAttribute<HttpPutAttribute>() != null)
            return "PUT";
        if (method.GetCustomAttribute<HttpPatchAttribute>() != null)
            return "PATCH";
        if (method.GetCustomAttribute<HttpDeleteAttribute>() != null)
            return "DELETE";
        return "ALL";
    }

    private static bool IsIgnoredAction(string actionName)
    {
        return false;
        //var ignored = new[] { "Index", "Create", "Edit", "Delete", "Update", "Insert",
        //    "Details", "GetAll", "GetById", "Execute", "Async" };
        //return ignored.Contains(actionName, StringComparer.OrdinalIgnoreCase);
    }

    private static string GeneratePolicyName(string controller, string action)
    {
        // Convert to singular: Users -> user, Orders -> order
        var resource = controller;
        //if (resource.EndsWith("s", StringComparison.OrdinalIgnoreCase) &&
        //    !resource.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
        //{
        //    resource = resource[..^1];
        //}
        return $"{resource.ToLower()}:{action.ToLower()}";

        // Map action to permission type
        //var actionType = action.ToLower() switch
        //{
        //    var a when a.Contains("get") || a.Contains("list") || a.Contains("getall") || a.Contains("getbyid") => "read",
        //    var a when a.Contains("create") || a.Contains("post") || a.Contains("add") => "create",
        //    var a when a.Contains("update") || a.Contains("put") || a.Contains("patch") || a.Contains("edit") => "update",
        //    var a when a.Contains("delete") || a.Contains("remove") => "delete",
        //    _ => "manage"
        //};
        //return $"{resource.ToLower()}.{actionType}"; 
    }

    private static bool HasExplicitPolicy(MethodInfo method)
    {
        var authorizeAttr = method.GetCustomAttribute<AuthorizeAttribute>();
        return authorizeAttr?.Policy != null;
    }
    private static string GetCategoryFromController(string controllerName)
    {
        return controllerName + "Management";
        //return controllerName switch
        //{
        //    "Auth" => "Authentication",
        //    "Users" => "UserManagement",
        //    "Roles" => "RoleManagement",
        //    "Permissions" => "PermissionManagement",
        //    "Sessions" => "SessionManagement",
        //    _ => controllerName + "Management"
        //};
    }

    //private static string GetActionDescription(MethodInfo method, string actionName)
    //{
    //    var displayAttr = method.GetCustomAttribute<DisplayAttribute>();
    //    return displayAttr?.Name ?? displayAttr?.Description ?? actionName;
    //}
}
