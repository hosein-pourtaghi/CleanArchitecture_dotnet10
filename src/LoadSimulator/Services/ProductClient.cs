using LoadSimulator.Infrastructure;
using LoadSimulator.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace LoadSimulator.Services;

/// <summary>
/// HTTP client for product operations
/// </summary>
public class ProductClient : IProductClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductClient> _logger;

    public ProductClient(HttpClient httpClient, ILogger<ProductClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<ProductDto>?> GetProductsAsync(
        int page = 1,
        int pageSize = 50,
        string? jwtToken = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/api/products?page={page}&pageSize={pageSize}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrEmpty(jwtToken))
            {
                request.AddBearerToken(jwtToken);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to get products: {StatusCode}",
                    response.StatusCode);
                return null;
            }

            var products = await response.DeserializeAsync<List<ProductDto>>(cancellationToken)
                .ConfigureAwait(false);

            return products ?? new List<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return null;
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(
        int productId,
        string? jwtToken = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/api/products/{productId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrEmpty(jwtToken))
            {
                request.AddBearerToken(jwtToken);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to get product {ProductId}: {StatusCode}",
                    productId,
                    response.StatusCode);
                return null;
            }

            var product = await response.DeserializeAsync<ProductDto>(cancellationToken)
                .ConfigureAwait(false);

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", productId);
            return null;
        }
    }
}
