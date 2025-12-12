using Application.Abstractions.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class AuthorizationRepository : IAuthorizationRepository
{
    private readonly IConfiguration _cfg;
    public AuthorizationRepository(IConfiguration cfg) => _cfg = cfg;

    public Task<bool> EvaluatePolicyAsync(string policyName, System.Security.Claims.ClaimsPrincipal user)
    {
        // Example: simplistic policy evaluation from config or DB
        // policyName format: Dynamic:RequireClaim:claimType:claimValue
        if (policyName.StartsWith("Dynamic:RequireClaim:", StringComparison.OrdinalIgnoreCase))
        {
            var parts = policyName.Split(':');
            if (parts.Length >= 4)
            {
                var type = parts[2];
                var val = parts[3];
                return Task.FromResult(user.HasClaim(c => c.Type == type && c.Value == val));
            }
        }
        // default deny
        return Task.FromResult(false);
    }
}
