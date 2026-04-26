using IdentityApi.Application.DTOs;
using SharedKernel;

namespace IdentityApi.Application.Interfaces;


/// <summary>
/// find policies dynamically 
/// no need to use [Authorize(policy:"xxx")]
/// </summary>
public interface IPolicyDiscoveryService
{
    Task<Result<int>> DiscoverAndRegisterPoliciesAsync();
    List<DiscoveredPolicy> DiscoverPolicies();
}
