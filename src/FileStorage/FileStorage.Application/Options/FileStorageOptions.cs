// FileStorage.Application/Options/FileStorageOptions.cs
namespace FileStorage.Application.Options;

public class FileStorageOptions
{
    public StorageOptions Storage { get; set; } = new();
    public ValidationOptions Validation { get; set; } = new();
    public SecurityOptions Security { get; set; } = new();
    public HistoryOptions History { get; set; } = new();
    public TrashOptions Trash { get; set; } = new();
    public AbandonedFilesOptions AbandonedFiles { get; set; } = new();
    public ThumbnailOptions Thumbnails { get; set; } = new();
    public PerformanceOptions Performance { get; set; } = new();
    public ResponseCacheOptions ResponseCache { get; set; } = new();
}

public class StorageOptions
{
    public string Provider { get; set; } = "Local";
    public string BasePath { get; set; } = "uploads";
    public string BaseUrl { get; set; } = "/files";
    public LocalStorageOptions Local { get; set; } = new();
    public S3StorageOptions? S3 { get; set; }
    public AzureStorageOptions? Azure { get; set; }
}

public class LocalStorageOptions
{
    public string RootPath { get; set; } = "wwwroot/uploads";
    public bool EnableDirectoryBrowsing { get; set; } = false;
}

public class S3StorageOptions
{
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public bool UsePathStyle { get; set; } = false;
}

public class AzureStorageOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "files";
    public string? CustomDomain { get; set; }
}

public class ValidationOptions
{
    public long MaxFileSizeBytes { get; set; } = 104857600; // 100MB
    public List<string> AllowedExtensions { get; set; } = new()
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".tiff",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".xml", ".json", ".zip", ".rar"
    };
    public List<string> BlockedExtensions { get; set; } = new()
    {
        ".exe", ".dll", ".bat", ".sh", ".ps1", ".cmd", ".msi",
        ".js", ".jsp", ".php", ".asp", ".aspx", ".cer", ".crt", ".p12", ".pfx",
        ".html", ".htm", ".svg" // SVG blocked by default for XSS
    };
    public bool EnableMagicByteValidation { get; set; } = true;
    public bool EnableExecutableScan { get; set; } = true;
    public bool EnableMimeTypeVerification { get; set; } = true;
    public long MaxImageDimension { get; set; } = 10000; // pixels
    public long SkipMagicBytesForSize { get; set; } = 1024; // Skip if file < 1KB
}

public class SecurityOptions
{
    public bool RequireAuthenticationForPrivateFiles { get; set; } = true;
    public bool RequireAuthenticationForPublicFiles { get; set; } = false;
    public bool LogAllAccess { get; set; } = true;
    public bool LogPublicFileAccess { get; set; } = true;
    public bool LogFailedAccess { get; set; } = true;
    public bool AllowPublicFilesWithoutOwner { get; set; } = true;
    public string DefaultAccessLevel { get; set; } = "Private";
    public int MaxAccessUrlExpirationMinutes { get; set; } = 60;
}

public class HistoryOptions
{
    public bool Enabled { get; set; } = true;
    public bool TrackAllChanges { get; set; } = true;
    public bool StorePreviousVersion { get; set; } = true;
    public bool StoreChangeReason { get; set; } = true;
    public bool StoreChangedBy { get; set; } = true;
    public bool StoreChangeDetails { get; set; } = true;
    public int RetentionDays { get; set; } = 365;
}

public class TrashOptions
{
    public bool Enabled { get; set; } = true;
    public bool MoveToTrashOnDelete { get; set; } = true;
    public int AutoDeleteAfterDays { get; set; } = 30;
    public bool PermanentDeleteRequiresAdmin { get; set; } = true;
    public bool AllowRestoreFromTrash { get; set; } = true;
}

public class AbandonedFilesOptions
{
    public bool Enabled { get; set; } = true;
    public string ScanInterval { get; set; } = "daily"; // never, hourly, daily, weekly
    public bool DefinitionNoOwner { get; set; } = true;
    public bool DefinitionOrphaned { get; set; } = true;
    public int OrphanedDays { get; set; } = 30;
    public int NoAccessInDays { get; set; } = 90;
    public int AutoDeleteAbandonedAfterDays { get; set; } = 0;
    public bool NotifyBeforeAutoDelete { get; set; } = true;
    public string? NotifyEmail { get; set; }
}

public class ThumbnailOptions
{
    public bool Enabled { get; set; } = true;
    public bool GenerateOnUpload { get; set; } = true;
    public string OutputFormat { get; set; } = "webp";
    public int Quality { get; set; } = 80;
    public ThumbnailSizeConfig Sizes { get; set; } = new();
    public long SkipForImagesLargerThan { get; set; } = 10485760; // 10MB
}

public class ThumbnailSizeConfig
{
    public ThumbnailSizeItem Small { get; set; } = new() { Width = 64, Height = 64 };
    public ThumbnailSizeItem Medium { get; set; } = new() { Width = 150, Height = 150 };
    public ThumbnailSizeItem Large { get; set; } = new() { Width = 300, Height = 300 };
}

public class ThumbnailSizeItem
{
    public int Width { get; set; }
    public int Height { get; set; }
}

public class PerformanceOptions
{
    public int BufferSize { get; set; } = 81920; // 80KB
    public bool EnableRangeRequests { get; set; } = true;
    public bool CachePublicFiles { get; set; } = true;
    public bool CachePrivateFiles { get; set; } = false;
    public int PublicCacheMaxAge { get; set; } = 31536000; // 1 year in seconds
}

public class ResponseCacheOptions
{
    public bool Enabled { get; set; } = true;
    public string PublicFilesCacheDuration { get; set; } = "01:00:00";
    public string PrivateFilesCacheDuration { get; set; } = "00:00:00";
    public string ThumbnailCacheDuration { get; set; } = "24:00:00";
}
