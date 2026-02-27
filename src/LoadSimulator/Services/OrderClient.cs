using LoadSimulator.Infrastructure;
using LoadSimulator.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace LoadSimulator.Services;

/// <summary>
/// HTTP client for order operations
/// </summary>
public class OrderClient : IOrderClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderClient> _logger;

    public OrderClient(HttpClient httpClient, ILogger<OrderClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CartDto?> CreateOrderAsync(
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders");
            request.AddBearerToken(jwtToken);
            request.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to create order: {StatusCode}",
                    response.StatusCode);
                return null;
            }

            var order = await response.DeserializeAsync<CartDto>(cancellationToken)
                .ConfigureAwait(false);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return null;
        }
    }

    public async Task<bool> AddOrderItemAsync(
        Guid orderId,
        Guid productId,
        int quantity,
        decimal? price,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/api/orders/{orderId}/items";
            var item = new { productId, quantity, price };
            var content = item.AsJsonContent();

            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            request.AddBearerToken(jwtToken);

            var response = await _httpClient.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to add order item to order {OrderId}: {StatusCode}",
                    orderId,
                    response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to order {OrderId}", orderId);
            return false;
        }
    }

    public async Task<bool> SubmitOrderAsync(
        Guid orderId,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/api/orders/{orderId}/submit";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.AddBearerToken(jwtToken);
            request.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to submit order {OrderId}: {StatusCode}",
                    orderId,
                    response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting order {OrderId}", orderId);
            return false;
        }
    }
}
