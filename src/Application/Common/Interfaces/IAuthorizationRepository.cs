namespace Application.Common.Interfaces;
public interface IAuthorizationRepository
{
    // Example: get policy rules from DB (role/claim based)
    Task<bool> EvaluatePolicyAsync(string policyName, System.Security.Claims.ClaimsPrincipal user);
}
