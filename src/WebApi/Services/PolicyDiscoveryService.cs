using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Infrastructure.Authorization;

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

    // Filter by namespace pattern (customize this)
    private const string ApiNamespacePattern = "API.Controllers";

    public PolicyDiscoveryService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public List<DiscoveredPolicy> DiscoverPolicies()
    {
        var policies = new List<DiscoveredPolicy>();

        var controllerTypes = Assembly.GetEntryAssembly()?.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

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

                // Check if already has explicit [Authorize] with policy
                var hasExplicitPolicy = HasExplicitPolicy(method);

                if (!hasExplicitPolicy)
                {
                    policies.Add(new DiscoveredPolicy
                    {
                        PolicyName = policyName,
                        Description = actionName, // Action name as description
                        Category = category,
                        Controller = controllerName,
                        Action = actionName,
                        HttpMethod = httpMethod
                    });
                }
            }
        }

        return policies.DistinctBy(p => p.PolicyName).ToList();
    }

    public Task<Result<int>> DiscoverAndRegisterPoliciesAsync()
    {
        try
        {
            var discoveredPolicies = DiscoverPolicies();

            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            // Get AuthorizationOptions
            var authorizationOptions = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>();
            var options = authorizationOptions.Value;

            var registeredCount = 0;

            foreach (var policy in discoveredPolicies)
            {
                var policyKey = $"Permission:{policy.PolicyName}";

                // Check if policy already exists
                if (options.GetPolicy(policyKey) == null)
                { 
                    options.AddPolicy(policyKey, builder =>
                    {
                        builder.AddRequirements(new PermissionRequirement(policy.PolicyName));
                    });
                    registeredCount++;
                }
            }

            return Task.FromResult(Result.Success(registeredCount));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<int>($"Failed to register policies: {ex.Message}"));
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
        if (resource.EndsWith("s", StringComparison.OrdinalIgnoreCase) &&
            !resource.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
        {
            resource = resource[..^1];
        }

        // Map action to permission type
        var actionType = action.ToLower() switch
        {
            var a when a.Contains("get") || a.Contains("list") || a.Contains("getall") || a.Contains("getbyid") => "read",
            var a when a.Contains("create") || a.Contains("post") || a.Contains("add") => "create",
            var a when a.Contains("update") || a.Contains("put") || a.Contains("patch") || a.Contains("edit") => "update",
            var a when a.Contains("delete") || a.Contains("remove") => "delete",
            _ => "manage"
        };

        return $"{resource.ToLower()}.{actionType}";
    }

    private static bool HasExplicitPolicy(MethodInfo method)
    {
        var authorizeAttr = method.GetCustomAttribute<AuthorizeAttribute>();
        return authorizeAttr?.Policy != null;
    }
    private static string GetCategoryFromController(string controllerName)
    {
        return controllerName switch
        {
            "Auth" => "Authentication",
            "Users" => "UserManagement",
            "Roles" => "RoleManagement",
            "Permissions" => "PermissionManagement",
            "Sessions" => "SessionManagement",
            _ => controllerName + "Management"
        };
    }

}
