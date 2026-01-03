namespace Web.Api.Models.Authentication;

/// <summary>
/// Standardized API error response model.
/// Returned for all error scenarios with consistent structure.
/// </summary>
public sealed class ApiErrorResponse
{
    /// <summary>
    /// HTTP status code of the error.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Error title or type identifier.
    /// Examples: "UNAUTHORIZED", "VALIDATION_ERROR", "NOT_FOUND"
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Detailed error message describing what went wrong.
    /// </summary>
    public required string Detail { get; set; }

    /// <summary>
    /// Unique identifier for this error instance for support/debugging.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Detailed validation errors if applicable.
    /// Keys are field names, values are lists of error messages.
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Timestamp when the error occurred (ISO 8601 format).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
