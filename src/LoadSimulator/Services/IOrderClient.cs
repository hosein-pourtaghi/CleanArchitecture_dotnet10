using LoadSimulator.Models.DTOs;

namespace LoadSimulator.Services;

/// <summary>
/// Interface for order operations against main API
/// </summary>
public interface IOrderClient
{
    Task<OrderDto?> CreateOrderAsync(
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<bool> AddOrderItemAsync(
        int orderId,
        int productId,
        int quantity,
        decimal price,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<bool> SubmitOrderAsync(
        int orderId,
        string jwtToken,
        CancellationToken cancellationToken = default);
}
