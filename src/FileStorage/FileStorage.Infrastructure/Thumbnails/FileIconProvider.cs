// FileStorage.Infrastructure/Thumbnails/FileIconProvider.cs
using FileStorage.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileStorage.Infrastructure.Thumbnails;

public class FileIconProvider : IFileIconProvider
{
    private readonly string _iconsPath;
    private readonly ILogger<FileIconProvider> _logger;

    private static readonly Dictionary<string, string> IconMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".pdf", "pdf" },
        { ".doc", "word" },
        { ".docx", "word" },
        { ".xls", "excel" },
        { ".xlsx", "excel" },
        { ".ppt", "powerpoint" },
        { ".pptx", "powerpoint" },
        { ".txt", "text" },
        { ".csv", "csv" },
        { ".zip", "archive" },
        { ".rar", "archive" },
        { ".7z", "archive" },
        { ".xml", "code" },
        { ".json", "code" },
        { ".html", "code" },
        { ".css", "code" },
        { ".js", "code" },
        { ".ts", "code" },
        { ".jpg", "image" },
        { ".jpeg", "image" },
        { ".png", "image" },
        { ".gif", "image" },
        { ".webp", "image" },
        { ".bmp", "image" },
        { ".mp3", "audio" },
        { ".wav", "audio" },
        { ".mp4", "video" },
        { ".avi", "video" },
        { ".mov", "video" },
    };

    public FileIconProvider(
        string iconsPath,
        ILogger<FileIconProvider>? logger = null)
    {
        _iconsPath = iconsPath;
        _logger = logger;
    }

    public async Task<Stream?> GetIconAsync(
        string extension,
        CancellationToken cancellationToken = default)
    {
        var iconName = IconMap.TryGetValue(extension, out var name) ? name : "default";
        var iconPath = Path.Combine(_iconsPath, $"{iconName}.svg");

        if (!File.Exists(iconPath))
        {
            _logger.LogWarning("Icon not found: {IconPath}", iconPath);
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(iconPath, cancellationToken);
        return new MemoryStream(bytes);
    }

    public async Task<string?> GetIconDataUriAsync(
        string extension,
        CancellationToken cancellationToken = default)
    {
        var stream = await GetIconAsync(extension, cancellationToken);
        if (stream == null)
            return null;

        using (stream)
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken);
            var base64 = Convert.ToBase64String(ms.ToArray());
            return $"data:image/svg+xml;base64,{base64}";
        }
    }

    public bool HasIcon(string extension)
    {
        return IconMap.ContainsKey(extension);
    }
}
