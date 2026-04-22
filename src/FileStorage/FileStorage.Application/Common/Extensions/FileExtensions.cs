// FileStorage.Application/Common/Extensions/FileExtensions.cs
namespace FileStorage.Application.Common.Extensions;

public static class FileExtensions
{
    private static readonly Dictionary<string, string> MimeTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".bmp", "image/bmp" },
        { ".tiff", "image/tiff" },
        { ".webp", "image/webp" },
        { ".svg", "image/svg+xml" },
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".txt", "text/plain" },
        { ".csv", "text/csv" },
        { ".xml", "application/xml" },
        { ".json", "application/json" },
        { ".zip", "application/zip" },
        { ".rar", "application/x-rar-compressed" },
        { ".7z", "application/x-7z-compressed" },
        { ".html", "text/html" },
        { ".htm", "text/html" },
        { ".css", "text/css" },
        { ".js", "application/javascript" },
        { ".ts", "application/typescript" }
    };

    public static string GetMimeType(string extension)
    {
        return MimeTypeMap.TryGetValue(extension, out var mimeType)
            ? mimeType
            : "application/octet-stream";
    }

    public static string GetExtension(string fileName)
    {
        var lastDot = fileName.LastIndexOf('.');
        return lastDot >= 0 ? fileName.Substring(lastDot).ToLowerInvariant() : string.Empty;
    }

    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "unnamed";

        // Remove path traversal attempts
        var sanitized = Path.GetFileName(fileName);

        // Replace invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
            sanitized = sanitized.Replace(c, '_');

        // Limit length
        if (sanitized.Length > 200)
            sanitized = sanitized.Substring(0, 200);

        return sanitized;
    }

    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    public static bool IsImage(string contentType)
    {
        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsDocument(string contentType)
    {
        return contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("document", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("msword", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("excel", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("spreadsheet", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("presentation", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsArchive(string contentType)
    {
        return contentType.Contains("zip", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("rar", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("7z", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("archive", StringComparison.OrdinalIgnoreCase);
    }
}
