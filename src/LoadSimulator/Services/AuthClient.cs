using LoadSimulator.Infrastructure;
using LoadSimulator.Models.DTOs;
using System.Net.Mime;

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
            var firstName = userName;
            var lastName = userName;
            var request = new { email, password, firstName, lastName };
            var content = request.AsJsonContent();
                         
            // Added logging for registration request
            _logger.LogInformation(
                "Registering user {Email} at {Url}",
                email,
                _httpClient.BaseAddress?.ToString() + "/api/auth/register");

            var response = await _httpClient.PostAsync(
                "/api/auth/register",
                content,
                cancellationToken).ConfigureAwait(false);

            // Logging the response status code
            _logger.LogInformation(
                "Registration response: {StatusCode} : {ReasonPhrase}",
                response.StatusCode,
                response.ReasonPhrase
                );

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

            // Added logging for successful registration
            _logger.LogInformation(
                "User {Email} registered successfully",
                email);

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

    public async Task HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                "/health",
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Health check failed: {StatusCode}",
                    response.StatusCode);
            }
            else
            {
                _logger.LogInformation("Health check passed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check error");
        }
    }

    public async Task<string> GetSwaggerDocumentAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                "/swagger",
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to fetch swagger document: {StatusCode}",
                    response.StatusCode);
                return string.Empty;
            }

            var swaggerDoc = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            return swaggerDoc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching swagger document");
            return string.Empty;
        }
    }
}
