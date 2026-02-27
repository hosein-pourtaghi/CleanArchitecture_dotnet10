using System.Text.Json;
using LoadSimulator.Models.DTOs;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LoadSimulator.Services;

/// <summary>
/// Product caching service with Redis support
/// </summary>
public class ProductCacheService : IProductCacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<ProductCacheService> _logger;
    private const string CachePrefix = "products:";
    private const int CacheDurationSeconds = 300;

    public ProductCacheService(
        ILogger<ProductCacheService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _redis = redis;
    }

    public async Task<List<ProductDto>?> GetCachedProductsAsync(
        string cacheKey,
        Func<Task<List<ProductDto>?>> fallbackFactory,
        CancellationToken cancellationToken = default)
    {
        if (_redis == null)
        {
            _logger.LogDebug("Redis not configured, using fallback");
            return await fallbackFactory().ConfigureAwait(false);
        }

        try
        {
            var fullKey = CachePrefix + cacheKey;
            var db = _redis.GetDatabase();

            var cached = await db.StringGetAsync(fullKey).ConfigureAwait(false);

            if (cached.HasValue)
            {
                _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                var products = JsonSerializer.Deserialize<List<ProductDto>>(cached.ToString());
                return products;
            }

            _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
            var data = await fallbackFactory().ConfigureAwait(false);

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                await db.StringSetAsync(
                    fullKey,
                    json,
                    TimeSpan.FromSeconds(CacheDurationSeconds)).ConfigureAwait(false);
            }

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache error for key: {CacheKey}", cacheKey);
            return await fallbackFactory().ConfigureAwait(false);
        }
    }

    public async Task InvalidateCacheAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        if (_redis == null)
            return;

        try
        {
            var fullKey = CachePrefix + cacheKey;
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(fullKey).ConfigureAwait(false);
            _logger.LogDebug("Cache invalidated for key: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for key: {CacheKey}", cacheKey);
        }
    }
}
