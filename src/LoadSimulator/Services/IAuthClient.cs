using LoadSimulator.Models.DTOs;

namespace LoadSimulator.Services;

/// <summary>
/// Interface for authentication operations against main API
/// </summary>
public interface IAuthClient
{
    Task<UserSessionDto?> RegisterAsync(
        string email,
        string password,
        string userName,
        CancellationToken cancellationToken = default);

    Task<UserSessionDto?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}
