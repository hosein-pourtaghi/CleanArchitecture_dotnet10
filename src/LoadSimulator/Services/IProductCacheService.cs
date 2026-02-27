using LoadSimulator.Models.DTOs;

namespace LoadSimulator.Services;

/// <summary>
/// Interface for caching product data (optional Redis integration)
/// </summary>
public interface IProductCacheService
{
    Task<List<ProductDto>?> GetCachedProductsAsync(
        string cacheKey,
        Func<Task<List<ProductDto>?>> fallbackFactory,
        CancellationToken cancellationToken = default);

    Task InvalidateCacheAsync(string cacheKey, CancellationToken cancellationToken = default);
}
