using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options; 


namespace Application.Common.Authorization;

//public class PermissionProvider
//{
//    private readonly IHttpContextAccessor _httpContextAccessor;

//    public PermissionProvider(HttpContextAccessor httpContextAccessor)
//    {
//        _httpContextAccessor = httpContextAccessor;
//    }

//    public IEnumerable<string> GetPermissions()
//    {
//        var context = _httpContextAccessor.HttpContext;
//        if (context?.User == null)
//            return Enumerable.Empty<string>();

//        // Get permissions from claims
//        return context.User
//            .FindAll("permission")
//            .Select(c => c.Value)
//            .Distinct();
//    }

//    public bool HasPermission(string permission)
//    {
//        return GetPermissions().Contains(permission);
//    }

//    public bool HasAnyPermission(params string[] permissions)
//    {
//        var userPermissions = GetPermissions();
//        return permissions.Any(p => userPermissions.Contains(p));
//    }

//    public bool HasAllPermissions(params string[] permissions)
//    {
//        var userPermissions = GetPermissions();
//        return permissions.All(p => userPermissions.Contains(p));
//    }
//}

//// Application/Authorization/PermissionAuthorizationHandler.cs
//using Microsoft.AspNetCore.Authorization;

//namespace Application.Authorization;

//public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
//{
//    private readonly PermissionProvider _permissionProvider;

//    public PermissionAuthorizationHandler(PermissionProvider permissionProvider)
//    {
//        _permissionProvider = permissionProvider;
//    }

//    protected override Task HandleRequirementAsync(
//        AuthorizationHandlerContext context,
//        PermissionRequirement requirement)
//    {
//        if (context.User == null || !context.User.Identity?.IsAuthenticated == true)
//        {
//            return Task.CompletedTask;
//        }

//        if (requirement.Permissions == null || !requirement.Permissions.Any())
//        {
//            // No specific permissions required, allow if authenticated
//            context.Succeed(requirement);
//            return Task.CompletedTask;
//        }

//        if (_permissionProvider.HasAnyPermission(requirement.Permissions))
//        {
//            context.Succeed(requirement);
//        }

//        return Task.CompletedTask;
//    }
//}

//public class PermissionRequirement : IAuthorizationRequirement
//{
//    public string[] Permissions { get; }

//    public PermissionRequirement(params string[] permissions)
//    {
//        Permissions = permissions;
//    }
//}


//public class PermissionAuthorizationPolicyProvider
//    : DefaultAuthorizationPolicyProvider
//{
//    public PermissionAuthorizationPolicyProvider(
//        IOptions<AuthorizationOptions> options)
//        : base(options)
//    {
//    }

//    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
//    {
//        // Check if policy starts with "Permission:"
//        if (policyName.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
//        {
//            var permissions = policyName
//                .Substring("Permission:".Length)
//                .Split(',', StringSplitOptions.RemoveEmptyEntries)
//                .Select(p => p.Trim())
//                .ToArray();

//            var policy = new AuthorizationPolicyBuilder()
//                .AddRequirements(new PermissionRequirement(permissions))
//                .Build();

//            return policy;
//        }

//        return await base.GetPolicyAsync(policyName);
//    }
//}
