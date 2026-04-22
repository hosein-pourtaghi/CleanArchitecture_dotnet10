// FileStorage.Domain/Interfaces/IThumbnailGenerator.cs
namespace FileStorage.Domain.Interfaces;

/// <summary>
/// Interface for thumbnail generation.
/// </summary>
public interface IThumbnailGenerator
{
    /// <summary>
    /// Generate thumbnails for an image.
    /// </summary>
    Task<ThumbnailResult> GenerateAsync(
        Stream imageStream,
        string originalFileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if this generator can handle the file type.
    /// </summary>
    bool CanGenerate(string contentType, long fileSize);

    /// <summary>
    /// Get supported content types.
    /// </summary>
    IReadOnlyList<string> SupportedContentTypes { get; }
}

/// <summary>
/// Result of thumbnail generation.
/// </summary>
public class ThumbnailResult
{
    public bool Success { get; set; }
    public Dictionary<string, Stream> Thumbnails { get; set; } = new();
    public string? Error { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }

    public static ThumbnailResult Succeeded(Dictionary<string, Stream> thumbnails, int? width = null, int? height = null) =>
        new() { Success = true, Thumbnails = thumbnails, Width = width, Height = height };

    public static ThumbnailResult Failed(string error) =>
        new() { Success = false, Error = error };
}
