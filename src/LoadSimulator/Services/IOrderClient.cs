using LoadSimulator.Models.DTOs;

namespace LoadSimulator.Services;

/// <summary>
/// Interface for order operations against main API
/// </summary>
public interface IOrderClient
{
    Task<CartDto?> CreateOrderAsync(
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<bool> AddOrderItemAsync(
        Guid orderId,
        Guid productId,
        int quantity,
        decimal? price,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<bool> SubmitOrderAsync(
        Guid orderId,
        string jwtToken,
        CancellationToken cancellationToken = default);
}
