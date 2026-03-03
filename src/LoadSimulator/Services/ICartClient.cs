using LoadSimulator.Models.DTOs;

namespace LoadSimulator.Services;

/// <summary>
/// Interface for cart operations against main API
/// </summary>
public interface ICartClient
{
    Task<CartDto?> CreateCartAsync(
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<bool> AddCartItemAsync(
        Guid cartId,
        Guid productId,
        int quantity,
        decimal? price,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<bool> SubmitCartAsync(
        Guid cartId,
        string jwtToken,
        CancellationToken cancellationToken = default);
}
