// FileStorage.Domain/Interfaces/IFileStorageProvider.cs
namespace FileStorage.Domain.Interfaces;

/// <summary>
/// Abstraction for file storage providers (Local, S3, Azure, etc.)
/// </summary>
public interface IFileStorageProvider
{
    /// <summary>
    /// Save a file to storage.
    /// </summary>
    Task<StorageResult> SaveAsync(Stream stream, string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a file from storage.
    /// </summary>
    Task<Stream?> GetAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file from storage.
    /// </summary>
    Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a file exists.
    /// </summary>
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copy a file to a new location.
    /// </summary>
    Task<string> CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move a file to a new location.
    /// </summary>
    Task<string> MoveAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file metadata.
    /// </summary>
    Task<FileMetadataResult> GetMetadataAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a temporary/pre-signed URL for accessing the file.
    /// </summary>
    Task<string?> GetAccessUrlAsync(string path, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the base URL for this storage provider.
    /// </summary>
    string BaseUrl { get; }
}

/// <summary>
/// Result of a storage operation.
/// </summary>
public class StorageResult
{
    public bool Success { get; set; }
    public string? Path { get; set; }
    public string? Error { get; set; }
    public long BytesWritten { get; set; }

    public static StorageResult Succeeded(string path, long bytesWritten) =>
        new() { Success = true, Path = path, BytesWritten = bytesWritten };

    public static StorageResult Failed(string error) =>
        new() { Success = false, Error = error };
}

/// <summary>
/// File metadata from storage.
/// </summary>
public class FileMetadataResult
{
    public bool Success { get; set; }
    public string? Path { get; set; }
    public long Size { get; set; }
    public DateTime? LastModified { get; set; }
    public string? ContentType { get; set; }
    public string? Error { get; set; }

    public static FileMetadataResult Succeeded(string path, long size, DateTime? lastModified, string? contentType = null) =>
        new() { Success = true, Path = path, Size = size, LastModified = lastModified, ContentType = contentType };

    public static FileMetadataResult Failed(string error) =>
        new() { Success = false, Error = error };
}
