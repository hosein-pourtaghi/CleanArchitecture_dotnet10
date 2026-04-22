// FileStorage.Application/Services/IFileManagerService.cs
using Application.Common.DTOs.Shared;
using FileStorage.Application.DTOs.Requests;
using FileStorage.Application.DTOs.Responses;
using FileStorage.Domain.Enums;
using SharedKernel;

namespace FileStorage.Application.Services;

/// <summary>
/// Main service interface for file management operations.
/// </summary>
public interface IFileManagerService
{
    #region Upload Operations

    Task<Result<FileUploadResponse>> UploadAsync(UploadFileRequest request, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<Result<FileUploadResponse>> UploadStreamAsync(UploadFileStreamRequest request, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<Result<BatchUploadResponse>> UploadBatchAsync(UploadBatchRequest request, Guid? userId = null, CancellationToken cancellationToken = default);

    #endregion

    #region Download Operations

    Task<Result<FileDownloadResponse>> DownloadAsync(Guid fileId, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<Result<FileDownloadResponse>> DownloadStreamAsync(Guid fileId, Guid? userId = null, RangeItem? range = null, CancellationToken cancellationToken = default);
    Task<Result<FileAccessUrlResponse>> GetAccessUrlAsync(Guid fileId, TimeSpan? expiration = null, Guid? userId = null, CancellationToken cancellationToken = default);

    #endregion

    #region Thumbnail & Icon Operations

    Task<Result<FileDownloadResponse>> GetThumbnailAsync(Guid fileId, ThumbnailSize size = ThumbnailSize.Medium, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<Result<FileDownloadResponse>> GetFileIconAsync(string extension, CancellationToken cancellationToken = default);

    #endregion

    #region Metadata Operations

    Task<Result<FileMetadataResponse>> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<FileMetadataResponse>>> GetFilesByOwnerAsync(Guid ownerId, string ownerType, CancellationToken cancellationToken = default);

    #endregion

    #region Management Operations

    Task<Result<FileMetadataResponse>> UpdateAsync(Guid fileId, UpdateFileRequest request, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<Result<FileUploadResponse>> ReplaceAsync(Guid fileId, ReplaceFileRequest request, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid fileId, Guid? userId = null, string? reason = null, CancellationToken cancellationToken = default);
    Task<Result> RestoreAsync(Guid fileId, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<Result> PermanentDeleteAsync(Guid fileId, Guid? userId = null, CancellationToken cancellationToken = default);

    #endregion

    #region Access Control Operations

    Task<Result<FilePermissionResponse>> GrantAccessAsync(Guid fileId, GrantAccessRequest request, Guid grantedById, CancellationToken cancellationToken = default);
    Task<Result> RevokeAccessAsync(Guid fileId, Guid userId, Guid revokedById, CancellationToken cancellationToken = default);
    Task<Result<bool>> CanAccessAsync(Guid fileId, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<FilePermissionResponse>>> GetPermissionsAsync(Guid fileId, CancellationToken cancellationToken = default);

    #endregion

    #region History & Audit Operations

    Task<Result<IReadOnlyList<FileHistoryResponse>>> GetHistoryAsync(Guid fileId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<FileAccessLogResponse>>> GetAccessLogsAsync(Guid fileId, int skip = 0, int take = 100, CancellationToken cancellationToken = default);

    #endregion

    #region Search Operations

    Task<Result<PaginatedResult<FileMetadataResponse>>> SearchAsync(FileSearchRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaginatedResult<AbandonedFileResponse>>> GetAbandonedFilesAsync(AbandonedFilesRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<FileMetadataResponse>>> GetDeletedFilesAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default);

    #endregion

    #region Cleanup Operations

    Task<Result<int>> EmptyTrashAsync(Guid? userId = null, CancellationToken cancellationToken = default);
    Task<Result<int>> DeleteAbandonedFilesAsync(AbandonedFilesRequest request, Guid? userId = null, bool dryRun = false, CancellationToken cancellationToken = default);
    Task<Result<int>> CleanupExpiredTrashAsync(CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Represents a byte range for partial content requests.
/// </summary>
public class RangeItem
{
    public long Start { get; set; }
    public long End { get; set; }

    public long Length => End - Start + 1;
}
