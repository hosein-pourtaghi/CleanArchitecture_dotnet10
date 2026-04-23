// FileStorage.Api/Controllers/FilesController.cs
using System.Security.Claims;
using Application.Common.DTOs.Shared;
using FileStorage.Application.DTOs.Requests;
using FileStorage.Application.DTOs.Responses;
using FileStorage.Application.Services;
using FileStorage.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedApi.Controllers;

namespace FileStorage.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
public class FilesController : ApiController
{
    private readonly IFileManagerService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileManagerService fileService,
        ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    #region Upload

    /// <summary>
    /// Upload a single file
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(104857600)] // 100MB
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> Upload(
        [FromForm] UploadFileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.UploadAsync(request, userId, cancellationToken);

        return HandleCreatedResult(result, "GetById", x => x.Id);
    }

    /// <summary>
    /// Upload multiple files as a batch
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(524288000)] // 500MB total
    [ProducesResponseType(typeof(BatchUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadBatch(
        [FromForm] UploadBatchRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.UploadBatchAsync(request, userId, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Stream upload for large files
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(104857600)]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> UploadStream(
        [FromBody] UploadFileStreamRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.UploadStreamAsync(request, userId, cancellationToken);

        return HandleCreatedResult(result, "GetById", x => x.Id);
    }

    #endregion

    #region Download

    /// <summary>
    /// Download a file
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous] // Access control handled in service
    [ProducesResponseType(typeof(FileDownloadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.DownloadAsync(id, userId, cancellationToken);

        return HandleResult(result, response =>
        {
            Response.Headers.Append("Content-Disposition",
                $"attachment; filename=\"{response.FileName}\"");
            Response.Headers.Append("Accept-Ranges", "bytes");

            if (!string.IsNullOrEmpty(response.ETag))
                Response.Headers.ETag = response.ETag;

            return File(response.Stream, response.ContentType, response.FileName);
        });
    }

    /// <summary>
    /// Stream download with range support
    /// </summary>
    [HttpGet("{id:guid}/stream")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status206PartialContent)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadStream(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.DownloadStreamAsync(id, userId, null, cancellationToken);

        return HandleResult(result, response =>
        {
            Response.Headers.Append("Content-Disposition",
                $"attachment; filename=\"{response.FileName}\"");
            return File(response.Stream, response.ContentType, response.FileName,
                enableRangeProcessing: true);
        });
    }

    /// <summary>
    /// Get a temporary access URL for the file
    /// </summary>
    [HttpGet("{id:guid}/url")]
    [ProducesResponseType(typeof(FileAccessUrlResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccessUrl(
        Guid id,
        [FromQuery] int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.GetAccessUrlAsync(
            id,
            TimeSpan.FromMinutes(expirationMinutes),
            userId,
            cancellationToken);

        return HandleResult(result);
    }

    #endregion

    #region Thumbnail & Icon

    /// <summary>
    /// Get file thumbnail
    /// </summary>
    [HttpGet("{id:guid}/thumbnail")]
    [AllowAnonymous]
    [ResponseCache(Duration = 86400)] // 24 hours
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetThumbnail(
        Guid id,
        [FromQuery] ThumbnailSize size = ThumbnailSize.Medium,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.GetThumbnailAsync(id, size, userId, cancellationToken);

        return HandleResult(result, response =>
            File(response.Stream, response.ContentType));
    }

    /// <summary>
    /// Get file type icon
    /// </summary>
    [HttpGet("icon/{extension}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 604800)] // 7 days
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIcon(
        string extension,
        CancellationToken cancellationToken)
    {
        var result = await _fileService.GetFileIconAsync(extension, cancellationToken);

        return HandleResult(result, response =>
            File(response.Stream, response.ContentType));
    }

    #endregion

    #region Metadata

    /// <summary>
    /// Get file metadata
    /// </summary>
    [HttpGet("{id:guid}/metadata")]
    [ProducesResponseType(typeof(FileMetadataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetadata(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _fileService.GetMetadataAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all files for an owner entity
    /// </summary>
    [HttpGet("owner/{ownerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<FileMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOwner(
        Guid ownerId,
        [FromQuery] string ownerType,
        CancellationToken cancellationToken)
    {
        var result = await _fileService.GetFilesByOwnerAsync(ownerId, ownerType, cancellationToken);
        return HandleResult(result);
    }

    #endregion

    #region Management

    /// <summary>
    /// Update file metadata
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FileMetadataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateFileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.UpdateAsync(id, request, userId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Replace file content (creates new version)
    /// </summary>
    [HttpPost("{id:guid}/replace")]
    [RequestSizeLimit(104857600)]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Replace(
        Guid id,
        [FromForm] ReplaceFileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.ReplaceAsync(id, request, userId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Soft delete (move to trash)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromQuery] string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.DeleteAsync(id, userId, reason, cancellationToken);
        return HandleResult(result, () => NoContent());
    }

    /// <summary>
    /// Restore from trash
    /// </summary>
    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Restore(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.RestoreAsync(id, userId, cancellationToken);
        return HandleResult(result, () => NoContent());
    }

    /// <summary>
    /// Permanently delete (admin only)
    /// </summary>
    [HttpDelete("{id:guid}/permanent")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PermanentDelete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.PermanentDeleteAsync(id, userId, cancellationToken);
        return HandleResult(result, () => NoContent());
    }

    #endregion

    #region Access Control

    /// <summary>
    /// Grant access to a user
    /// </summary>
    [HttpPost("{id:guid}/access")]
    [ProducesResponseType(typeof(FilePermissionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GrantAccess(
        Guid id,
        [FromBody] GrantAccessRequest request,
        CancellationToken cancellationToken)
    {
        var grantedById = GetCurrentUserId() ?? Guid.Empty;
        var result = await _fileService.GrantAccessAsync(id, request, grantedById, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Revoke user access
    /// </summary>
    [HttpDelete("{id:guid}/access/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeAccess(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var revokedById = GetCurrentUserId() ?? Guid.Empty;
        var result = await _fileService.RevokeAccessAsync(id, userId, revokedById, cancellationToken);
        return HandleResult(result, () => NoContent());
    }

    /// <summary>
    /// List all permissions for a file
    /// </summary>
    [HttpGet("{id:guid}/permissions")]
    [ProducesResponseType(typeof(IReadOnlyList<FilePermissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissions(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _fileService.GetPermissionsAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Check if current user can access a file
    /// </summary>
    [HttpGet("{id:guid}/can-access")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CanAccess(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.CanAccessAsync(id, userId, cancellationToken);
        return HandleResult(result);
    }

    #endregion

    #region History & Audit

    /// <summary>
    /// Get file modification history
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<FileHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _fileService.GetHistoryAsync(id, skip, take, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get file access logs
    /// </summary>
    [HttpGet("{id:guid}/access-logs")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<FileAccessLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccessLogs(
        Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _fileService.GetAccessLogsAsync(id, skip, take, cancellationToken);
        return HandleResult(result);
    }

    #endregion

    #region Search

    /// <summary>
    /// Search files with filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<FileMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] FileSearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _fileService.SearchAsync(request, cancellationToken);
        return HandlePaginatedResult(result);
    }

    /// <summary>
    /// Get files in trash
    /// </summary>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(IReadOnlyList<FileMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeleted(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _fileService.GetDeletedFilesAsync(skip, take, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get abandoned files
    /// </summary>
    [HttpGet("abandoned")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaginatedResult<AbandonedFileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAbandoned(
        [FromQuery] AbandonedFilesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _fileService.GetAbandonedFilesAsync(request, cancellationToken);
        return HandlePaginatedResult(result);
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Empty the trash
    /// </summary>
    [HttpPost("trash/empty")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EmptyTrash(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.EmptyTrashAsync(userId, cancellationToken);

        return HandleResult(result, () => NoContent());
    }

    /// <summary>
    /// Delete abandoned files
    /// </summary>
    [HttpPost("abandoned/cleanup")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CleanupAbandoned(
        [FromQuery] AbandonedFilesRequest request,
        [FromQuery] bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.DeleteAbandonedFilesAsync(
            request, userId, dryRun, cancellationToken);

        return HandleResult(result, () => NoContent());
    }

    #endregion

    #region Private Methods

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }

    #endregion
}
