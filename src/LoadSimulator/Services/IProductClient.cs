using LoadSimulator.Models.DTOs;

namespace LoadSimulator.Services;

/// <summary>
/// Interface for product operations against main API
/// </summary>
public interface IProductClient
{
    Task<List<ProductDto>?> GetProductsAsync(
        int page = 1,
        int pageSize = 50,
        string? jwtToken = null,
        CancellationToken cancellationToken = default);

    Task<ProductDto?> GetProductByIdAsync(
        int productId,
        string? jwtToken = null,
        CancellationToken cancellationToken = default);
}
