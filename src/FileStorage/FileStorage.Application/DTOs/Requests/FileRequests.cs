// FileStorage.Application/DTOs/Requests/FileRequests.cs
using FileStorage.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace FileStorage.Application.DTOs.Requests;

public class UploadFileRequest
{
    public IFormFile File { get; set; } = null!;
    public FileCategory Category { get; set; } = FileCategory.GeneralDocument;
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Private;
    public Guid? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public string? OwnerProperty { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool GenerateThumbnail { get; set; } = true;
}

public class UploadFileStreamRequest
{
    public Stream Stream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public FileCategory Category { get; set; } = FileCategory.GeneralDocument;
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Private;
    public Guid? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public string? OwnerProperty { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public class UploadBatchRequest
{
    public List<IFormFile> Files { get; set; } = new();
    public FileCategory Category { get; set; } = FileCategory.GeneralDocument;
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Private;
    public Guid? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public string? OwnerProperty { get; set; }
}

public class UpdateFileRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public AccessLevel? AccessLevel { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public string? OwnerProperty { get; set; }
    public Dictionary<string, string>? CustomProperties { get; set; }
}

public class ReplaceFileRequest
{
    public IFormFile File { get; set; } = null!;
    public string? ChangeReason { get; set; }
}

public class GrantAccessRequest
{
    public Guid UserId { get; set; }
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Private;
    public DateTime? ExpiresAt { get; set; }
    public string? Reason { get; set; }
}

public class FileSearchRequest
{
    public string? SearchTerm { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public FileCategory? Category { get; set; }
    public AccessLevel? AccessLevel { get; set; }
    public FileBucket? Bucket { get; set; }
    public string? ContentType { get; set; }
    public long? MinSize { get; set; }
    public long? MaxSize { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public bool IncludeDeleted { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string OrderBy { get; set; } = "CreatedAt";
    public bool OrderDescending { get; set; } = true;
}

public class AbandonedFilesRequest
{
    public bool IncludeNoOwner { get; set; } = true;
    public bool IncludeOrphaned { get; set; } = true;
    public int OrphanedDays { get; set; } = 30;
    public int NoAccessInDays { get; set; } = 90;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
