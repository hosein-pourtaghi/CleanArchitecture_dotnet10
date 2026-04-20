using Application.Common.Models;
using SharedKernel;

namespace Application.Common.Interfaces.Core;


/// <summary>
/// find policies dynamically 
/// no need to use [Authorize(policy:"xxx")]
/// </summary>
public interface IPolicyDiscoveryService
{
    Task<Result<int>> DiscoverAndRegisterPoliciesAsync();
    List<DiscoveredPolicy> DiscoverPolicies();
}
