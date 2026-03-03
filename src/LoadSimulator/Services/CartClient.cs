using LoadSimulator.Infrastructure;
using LoadSimulator.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace LoadSimulator.Services;

/// <summary>
/// HTTP client for cart operations
/// </summary>
public class CartClient : ICartClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CartClient> _logger;

    public CartClient(HttpClient httpClient, ILogger<CartClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CartDto?> CreateCartAsync(
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/carts");
            request.AddBearerToken(jwtToken);
            request.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to create cart: {StatusCode}",
                    response.StatusCode);
                return null;
            }

            var cart = await response.DeserializeAsync<CartDto>(cancellationToken)
                .ConfigureAwait(false);

            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cart");
            return null;
        }
    }

    public async Task<bool> AddCartItemAsync(
        Guid cartId,
        Guid productId,
        int quantity,
        decimal? price,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/api/carts/{cartId}/items";
            var item = new { productId, quantity, price };
            var content = item.AsJsonContent();

            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            request.AddBearerToken(jwtToken);

            var response = await _httpClient.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to add cart item to cart {CartId}: {StatusCode}",
                    cartId,
                    response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart {CartId}", cartId);
            return false;
        }
    }

    public async Task<bool> SubmitCartAsync(
        Guid cartId,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/api/carts/{cartId}/submit";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.AddBearerToken(jwtToken);
            request.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to submit cart {CartId}: {StatusCode}",
                    cartId,
                    response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting cart {CartId}", cartId);
            return false;
        }
    }
}
