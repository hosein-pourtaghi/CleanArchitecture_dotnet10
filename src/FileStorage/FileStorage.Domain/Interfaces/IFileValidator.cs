// FileStorage.Domain/Interfaces/IFileValidator.cs
namespace FileStorage.Domain.Interfaces;

/// <summary>
/// Interface for file validation.
/// </summary>
public interface IFileValidator
{
    /// <summary>
    /// Validate a file upload request.
    /// </summary>
    Task<ValidationResultLib> ValidateAsync(
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate just the file extension.
    /// </summary>
    bool IsExtensionAllowed(string extension);

    /// <summary>
    /// Check if extension is blocked.
    /// </summary>
    bool IsExtensionBlocked(string extension);

    /// <summary>
    /// Get allowed extensions.
    /// </summary>
    IReadOnlyList<string> GetAllowedExtensions();

    /// <summary>
    /// Get blocked extensions.
    /// </summary>
    IReadOnlyList<string> GetBlockedExtensions();
}

/// <summary>
/// Result of file validation.
/// </summary>
public class ValidationResultLib
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();

    public static ValidationResultLib Success() => new() { IsValid = true };

    public static ValidationResultLib Failure(params ValidationError[] errors) =>
        new() { IsValid = false, Errors = errors.ToList() };

    public static ValidationResultLib Failure(IEnumerable<ValidationError> errors) =>
        new() { IsValid = false, Errors = errors.ToList() };
}

/// <summary>
/// Validation error details.
/// </summary>
public class ValidationError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }

    public ValidationError() { }

    public ValidationError(string code, string message, string? field = null)
    {
        Code = code;
        Message = message;
        Field = field;
    }

    public static ValidationError FileTooLarge(long maxSize, long actualSize) =>
        new("FILE_TOO_LARGE", $"File size ({actualSize} bytes) exceeds maximum allowed ({maxSize} bytes)", "file");

    public static ValidationError ExtensionNotAllowed(string extension) =>
        new("EXTENSION_NOT_ALLOWED", $"File extension '{extension}' is not allowed", "file");

    public static ValidationError ExtensionBlocked(string extension) =>
        new("EXTENSION_BLOCKED", $"File extension '{extension}' is blocked for security", "file");

    public static ValidationError InvalidMimeType(string expected, string actual) =>
        new("INVALID_MIME_TYPE", $"Content type '{actual}' does not match expected '{expected}'", "contentType");

    public static ValidationError MagicBytesMismatch(string expected, string actual) =>
        new("MAGIC_BYTES_MISMATCH", $"File content does not match declared type. Expected {expected}, got {actual}", "file");

    public static ValidationError ExecutableDetected(string type) =>
        new("EXECUTABLE_DETECTED", $"Executable content detected ({type}). This file type is not allowed.", "file");

    public static ValidationError InvalidFileName(string fileName) =>
        new("INVALID_FILE_NAME", $"File name '{fileName}' contains invalid characters", "fileName");
}
