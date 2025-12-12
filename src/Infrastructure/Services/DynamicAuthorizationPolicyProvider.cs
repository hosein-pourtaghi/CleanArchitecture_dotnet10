using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;
public class DynamicAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;
    private readonly IAuthorizationRepository _repo;

    public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IAuthorizationRepository repo)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
        _repo = repo;
    }

    // If policy name starts with "Dynamic:", evaluate dynamically
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("Dynamic:", StringComparison.OrdinalIgnoreCase))
        {
            // create policy that uses a requirement which will be evaluated by handler
            var p = new AuthorizationPolicyBuilder().AddRequirements(new DynamicRequirement(policyName)).Build();
            return Task.FromResult<AuthorizationPolicy?>(p);
        }
        return _fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}

public class DynamicRequirement : IAuthorizationRequirement
{
    public string PolicyName { get; }
    public DynamicRequirement(string policyName) => PolicyName = policyName;
}

public class DynamicHandler : AuthorizationHandler<DynamicRequirement>
{
    private readonly IAuthorizationRepository _repo;
    public DynamicHandler(IAuthorizationRepository repo) => _repo = repo;
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DynamicRequirement requirement)
    {
        var ok = await _repo.EvaluatePolicyAsync(requirement.PolicyName, context.User);
        if (ok) context.Succeed(requirement);
    }
}
