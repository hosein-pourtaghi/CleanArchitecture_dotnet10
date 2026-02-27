using System.Text.Json;
using LoadSimulator.Infrastructure;
using LoadSimulator.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace LoadSimulator.Services;

/// <summary>
/// HTTP client for authentication operations
/// </summary>
public class AuthClient : IAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthClient> _logger;

    public AuthClient(HttpClient httpClient, ILogger<AuthClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserSessionDto?> RegisterAsync(
        string email,
        string password,
        string userName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { email, password, userName };
            var content = request.AsJsonContent();

            var response = await _httpClient.PostAsync(
                "/api/auth/register",
                content,
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Registration failed for {Email}: {StatusCode}",
                    email,
                    response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);
            
            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            if (!root.TryGetProperty("token", out var tokenElement) ||
                !root.TryGetProperty("expiresIn", out var expiresElement))
            {
                _logger.LogWarning("Invalid registration response format");
                return null;
            }

            var token = tokenElement.GetString() ?? string.Empty;
            var expiresInSeconds = expiresElement.GetInt32();

            return new UserSessionDto
            {
                Email = email,
                JwtToken = token,
                LoginTime = DateTime.UtcNow,
                TokenExpiryTime = DateTime.UtcNow.AddSeconds(expiresInSeconds)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error for {Email}", email);
            return null;
        }
    }

    public async Task<UserSessionDto?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { email, password };
            var content = request.AsJsonContent();

            var response = await _httpClient.PostAsync(
                "/api/auth/login",
                content,
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Login failed for {Email}: {StatusCode}",
                    email,
                    response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);
            
            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            if (!root.TryGetProperty("token", out var tokenElement) ||
                !root.TryGetProperty("expiresIn", out var expiresElement))
            {
                _logger.LogWarning("Invalid login response format");
                return null;
            }

            var token = tokenElement.GetString() ?? string.Empty;
            var expiresInSeconds = expiresElement.GetInt32();

            return new UserSessionDto
            {
                Email = email,
                JwtToken = token,
                LoginTime = DateTime.UtcNow,
                TokenExpiryTime = DateTime.UtcNow.AddSeconds(expiresInSeconds)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {Email}", email);
            return null;
        }
    }
}
