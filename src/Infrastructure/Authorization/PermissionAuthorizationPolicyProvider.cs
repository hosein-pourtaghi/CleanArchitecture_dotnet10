using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authorization; 
public class PermissionAuthorizationPolicyProvider
    : IAuthorizationPolicyProvider
{
    private readonly IOptions<AuthorizationOptions> _options;
    private readonly IMemoryCache _cache;
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

    private const string PolicyCacheKey = "AuthPolicy_";
    private static readonly TimeSpan PolicyCacheDuration = TimeSpan.FromHours(1);

    // Default policy with empty requirement (for [Authorize] without policy)
    private readonly AuthorizationPolicy _defaultPolicy;

    public PermissionAuthorizationPolicyProvider(
        IOptions<AuthorizationOptions> options,
        IMemoryCache cache)
    {
        _options = options;
        _cache = cache;
        _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);

        // Create default policy that uses our requirement with NO permissions
        // This allows auto-detection from route
        _defaultPolicy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement())
            .Build();
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        // Return our custom default policy instead of the built-in one
        return Task.FromResult(_defaultPolicy);
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Empty policy name = default policy
        if (string.IsNullOrEmpty(policyName))
        {
            return Task.FromResult<AuthorizationPolicy?>(_defaultPolicy);
        }

        // Check cache first
        var cacheKey = $"{PolicyCacheKey}{policyName}";

        if (_cache.TryGetValue(cacheKey, out AuthorizationPolicy? cachedPolicy))
        {
            return Task.FromResult(cachedPolicy);
        }

        AuthorizationPolicy? policy = null;

        // Handle "permission:users:getall" format
        if (policyName.StartsWith("permission:", StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName.Substring("permission:".Length);

            policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
        }
        // Handle "users:getall" format (discovered policy)
        else if (policyName.Contains(':'))
        {
            policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }
        else
        {
            // Fallback to default provider for other policies
            return _fallbackProvider.GetPolicyAsync(policyName);
        }

        // Cache the policy
        if (policy != null)
        {
            _cache.Set(cacheKey, policy, PolicyCacheDuration);
        }

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
