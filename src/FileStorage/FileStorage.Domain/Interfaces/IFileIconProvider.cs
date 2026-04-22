// FileStorage.Domain/Interfaces/IFileIconProvider.cs
namespace FileStorage.Domain.Interfaces;

/// <summary>
/// Interface for file type icons.
/// </summary>
public interface IFileIconProvider
{
    /// <summary>
    /// Get icon for a file extension.
    /// </summary>
    Task<Stream?> GetIconAsync(string extension, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get icon as base64 data URI.
    /// </summary>
    Task<string?> GetIconDataUriAsync(string extension, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if icon exists for extension.
    /// </summary>
    bool HasIcon(string extension);
}
