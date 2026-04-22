// FileStorage.Infrastructure/Storage/Providers/LocalStorageProvider.cs
using FileStorage.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileStorage.Infrastructure.Storage.Providers;

public class LocalStorageProvider : IFileStorageProvider
{
    private readonly string _rootPath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalStorageProvider> _logger;
    private readonly int _bufferSize;

    public string BaseUrl => _baseUrl;

    public LocalStorageProvider(
        string rootPath,
        string baseUrl,
        int bufferSize = 81920,
        ILogger<LocalStorageProvider>? logger = null)
    {
        _rootPath = Path.GetFullPath(rootPath);
        _baseUrl = baseUrl.TrimEnd('/');
        _bufferSize = bufferSize;
        _logger = logger;

        // Ensure root directory exists
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<StorageResult> SaveAsync(
        Stream stream,
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_rootPath, path);
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await using var fileStream = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                _bufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            var bytesWritten = 0L;
            var buffer = new byte[_bufferSize];
            int read;

            while ((read = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                bytesWritten += read;
            }

            _logger.LogInformation("File saved successfully: {Path}, Size: {Size} bytes", path, bytesWritten);

            return StorageResult.Succeeded(path, bytesWritten);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file: {Path}", path);
            return StorageResult.Failed(ex.Message);
        }
    }

    public async Task<Stream?> GetAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_rootPath, path);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found: {Path}", path);
                return null;
            }

            var memoryStream = new MemoryStream();
            await using var fileStream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                _bufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            await fileStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read file: {Path}", path);
            return null;
        }
    }

    public Task<bool> DeleteAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_rootPath, path);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {Path}", path);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {Path}", path);
            return Task.FromResult(false);
        }
    }

    public Task<bool> ExistsAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, path);
        return Task.FromResult(File.Exists(fullPath));
    }

    public async Task<string> CopyAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        var sourceFullPath = Path.Combine(_rootPath, sourcePath);
        var destFullPath = Path.Combine(_rootPath, destinationPath);
        var destDirectory = Path.GetDirectoryName(destFullPath);

        if (!string.IsNullOrEmpty(destDirectory))
            Directory.CreateDirectory(destDirectory);

        await using var sourceStream = new FileStream(
            sourceFullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            _bufferSize,
            FileOptions.Asynchronous);

        await using var destStream = new FileStream(
            destFullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            _bufferSize,
            FileOptions.Asynchronous);

        await sourceStream.CopyToAsync(destStream, cancellationToken);

        _logger.LogInformation("File copied from {Source} to {Destination}", sourcePath, destinationPath);

        return destinationPath;
    }

    public async Task<string> MoveAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        var sourceFullPath = Path.Combine(_rootPath, sourcePath);
        var destFullPath = Path.Combine(_rootPath, destinationPath);
        var destDirectory = Path.GetDirectoryName(destFullPath);

        if (!string.IsNullOrEmpty(destDirectory))
            Directory.CreateDirectory(destDirectory);

        // Check if source exists
        if (!File.Exists(sourceFullPath))
        {
            _logger.LogWarning("Source file not found for move: {Path}", sourcePath);
            throw new FileNotFoundException($"Source file not found: {sourcePath}");
        }

        // Use File.Move for atomic operation when within same volume
        File.Move(sourceFullPath, destFullPath, overwrite: true);

        _logger.LogInformation("File moved from {Source} to {Destination}", sourcePath, destinationPath);

        return destinationPath;
    }

    public Task<FileMetadataResult> GetMetadataAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_rootPath, path);

            if (!File.Exists(fullPath))
                return Task.FromResult(FileMetadataResult.Failed("File not found"));

            var fileInfo = new FileInfo(fullPath);

            return Task.FromResult(FileMetadataResult.Succeeded(
                path,
                fileInfo.Length,
                fileInfo.LastWriteTimeUtc,
                GetMimeType(fullPath)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file metadata: {Path}", path);
            return Task.FromResult(FileMetadataResult.Failed(ex.Message));
        }
    }

    public Task<string?> GetAccessUrlAsync(
        string path,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        // For local storage, return a direct path URL
        // In production, you might want to generate a signed URL or use a different approach
        var url = $"{_baseUrl}/{path}";
        return Task.FromResult<string?>(url);
    }

    private static string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
