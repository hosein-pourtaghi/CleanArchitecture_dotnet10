namespace Web.Api.Models.Authentication;

/// <summary>
/// Response model returned after successful user registration.
/// Contains the JWT authentication token for immediate API access.
/// </summary>
public sealed class RegisterResponse
{
    /// <summary>
    /// JWT authentication token to be used in subsequent API requests.
    /// Include this token in the Authorization header as: Authorization: Bearer {token}
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// The type of the token (always "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Number of seconds until the token expires.
    /// Client should request a new token before this duration passes.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Human-readable expiration date/time in UTC format.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
