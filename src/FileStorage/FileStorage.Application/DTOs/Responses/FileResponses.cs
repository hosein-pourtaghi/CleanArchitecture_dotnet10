// FileStorage.Application/DTOs/Responses/FileResponses.cs
using FileStorage.Domain.Enums;
using FileStorage.Domain.ValueObjects;

namespace FileStorage.Application.DTOs.Responses;

public class FileUploadResponse
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    public FileCategory Category { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public string? OwnerProperty { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? DownloadUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FileMetadataResponse
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    public FileCategory Category { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public FileBucket Bucket { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public string? OwnerProperty { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Checksum { get; set; }
    public FileMetadata Metadata { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class FileDownloadResponse
{
    public Stream Stream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ETag { get; set; }
    public DateTime? LastModified { get; set; }
    public bool AcceptRanges { get; set; } = true;
}

public class FileHistoryResponse
{
    public Guid Id { get; set; }
    public Guid FileAttachmentId { get; set; }
    public FileAction Action { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public string? OwnerProperty { get; set; }
    public Guid? PreviousVersionId { get; set; }
    public Guid? NewVersionId { get; set; }
    public string? ChangeReason { get; set; }
    public FileChangeDetails? ChangeDetails { get; set; }
    public Guid? ChangedById { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FileAccessLogResponse
{
    public Guid Id { get; set; }
    public Guid FileAttachmentId { get; set; }
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public FileAccessAction Action { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FilePermissionResponse
{
    public Guid Id { get; set; }
    public Guid FileAttachmentId { get; set; }
    public Guid UserId { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public Guid? GrantedById { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Reason { get; set; }
    public bool IsValid { get; set; }
}

public class FileAccessUrlResponse
{
    public Guid FileId { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class BatchUploadResponse
{
    public List<FileUploadResponse> Successful { get; set; } = new();
    public List<BatchUploadError> Failed { get; set; } = new();
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public class BatchUploadError
{
    public string FileName { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

public class AbandonedFileResponse
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public FileCategory Category { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public string AbandonedReason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public DateTime? OwnerDeletedAt { get; set; }
}
