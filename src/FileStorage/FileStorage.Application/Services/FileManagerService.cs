// FileStorage.Application/Services/FileManagerService.cs
using System.Security.Cryptography;
using Application.Common.DTOs.Shared;
using FileStorage.Application.Common.Extensions;
using FileStorage.Application.DTOs.Requests;
using FileStorage.Application.DTOs.Responses;
using FileStorage.Application.Options;
using FileStorage.Domain.Entities;
using FileStorage.Domain.Enums;
using FileStorage.Domain.Interfaces;
using FileStorage.Domain.Interfaces.Repositories;
using FileStorage.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using SharedKernel; 

namespace FileStorage.Application.Services;

public class FileManagerService : IFileManagerService
{
    private readonly IFileAttachmentRepository _fileRepository;
    private readonly IFileHistoryRepository _historyRepository;
    private readonly IFileAccessLogRepository _accessLogRepository;
    private readonly IFileAccessPermissionRepository _permissionRepository;
    private readonly IFileStorageProvider _storageProvider;
    private readonly IFileValidator _fileValidator;
    private readonly IThumbnailGenerator _thumbnailGenerator;
    private readonly IFileIconProvider _iconProvider;
    private readonly FileStorageOptions _options;
    private readonly ICurrentUserService _currentUserService;

    public FileManagerService(
        IFileAttachmentRepository fileRepository,
        IFileHistoryRepository historyRepository,
        IFileAccessLogRepository accessLogRepository,
        IFileAccessPermissionRepository permissionRepository,
        IFileStorageProvider storageProvider,
        IFileValidator fileValidator,
        IThumbnailGenerator thumbnailGenerator,
        IFileIconProvider iconProvider,
        IOptions<FileStorageOptions> options,
        ICurrentUserService currentUserService)
    {
        _fileRepository = fileRepository;
        _historyRepository = historyRepository;
        _accessLogRepository = accessLogRepository;
        _permissionRepository = permissionRepository;
        _storageProvider = storageProvider;
        _fileValidator = fileValidator;
        _thumbnailGenerator = thumbnailGenerator;
        _iconProvider = iconProvider;
        _options = options.Value;
        _currentUserService = currentUserService;
    }

    #region Upload Operations

    public async Task<Result<FileUploadResponse>> UploadAsync(
        UploadFileRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var file = request.File;
        var fileName = FileExtensions.SanitizeFileName(file.FileName);
        var extension = FileExtensions.GetExtension(fileName);
        var contentType = file.ContentType;

        // Validate
        using var stream = file.OpenReadStream();
        var validationResult = await _fileValidator.ValidateAsync(
            stream, fileName, contentType, file.Length, cancellationToken);

        if (!validationResult.IsValid)
        {
            var error = validationResult.Errors.First();
            return Result.Failure<FileUploadResponse>(error.Code, error.Message);
        }

        // Generate unique stored filename
        var storedFileName = $"{Guid.NewGuid():N}_{Path.GetFileNameWithoutExtension(fileName)}{extension}";
        var now = DateTime.UtcNow;
        var storagePath = GenerateStoragePath(request.AccessLevel, request.Category, now, storedFileName);

        // Calculate checksum
        stream.Position = 0;
        var checksum = await CalculateChecksumAsync(stream);

        // Save to storage
        stream.Position = 0;
        var saveResult = await _storageProvider.SaveAsync(stream, storagePath, cancellationToken);
        if (!saveResult.Success)
            return Result.Failure<FileUploadResponse>("STORAGE_ERROR", saveResult.Error ?? "Failed to save file");

        // Create entity
        var metadata = FileMetadata.Create(request.Title, request.Description);
        var fileAttachment = FileAttachment.Create(
            originalFileName: fileName,
            storedFileName: storedFileName,
            fileExtension: extension,
            contentType: contentType,
            fileSize: file.Length,
            storagePath: storagePath,
            category: request.Category,
            accessLevel: request.AccessLevel,
            ownerId: request.OwnerId,
            ownerType: request.OwnerType,
            ownerProperty: request.OwnerProperty,
            checksum: checksum,
            metadata: metadata);

        fileAttachment.SetCreatedBy(userId ?? _currentUserService.GetUserId());

        // Generate thumbnail if image
        if (request.GenerateThumbnail && _thumbnailGenerator.CanGenerate(contentType, file.Length))
        {
            stream.Position = 0;
            var thumbnailResult = await _thumbnailGenerator.GenerateAsync(stream, fileName, cancellationToken);
            if (thumbnailResult.Success)
            {
                var thumbnailPath = GenerateThumbnailPath(storagePath);
                foreach (var (size, thumbnailStream) in thumbnailResult.Thumbnails)
                {
                    await _storageProvider.SaveAsync(thumbnailStream, $"{thumbnailPath}_{size}", cancellationToken);
                }
                fileAttachment.SetThumbnailPath(thumbnailPath);
            }
        }

        await _fileRepository.AddAsync(fileAttachment, cancellationToken);

        // Record history
        await RecordHistoryAsync(
            fileAttachment,
            FileAction.Created,
            userId,
            FileChangeDetails.CreateForUpload(),
            cancellationToken);

        return Result.Success(MapToUploadResponse(fileAttachment));
    }

    public async Task<Result<FileUploadResponse>> UploadStreamAsync(
        UploadFileStreamRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var fileName = FileExtensions.SanitizeFileName(request.FileName);
        var extension = FileExtensions.GetExtension(fileName);

        // Validate
        var validationResult = await _fileValidator.ValidateAsync(
            request.Stream, fileName, request.ContentType, request.FileSize, cancellationToken);

        if (!validationResult.IsValid)
        {
            var error = validationResult.Errors.First();
            return Result.Failure<FileUploadResponse>(error.Code, error.Message);
        }

        // Generate unique stored filename
        var storedFileName = $"{Guid.NewGuid():N}_{Path.GetFileNameWithoutExtension(fileName)}{extension}";
        var now = DateTime.UtcNow;
        var storagePath = GenerateStoragePath(request.AccessLevel, request.Category, now, storedFileName);

        // Calculate checksum
        var checksum = await CalculateChecksumAsync(request.Stream);
        request.Stream.Position = 0;

        // Save to storage
        var saveResult = await _storageProvider.SaveAsync(request.Stream, storagePath, cancellationToken);
        if (!saveResult.Success)
            return Result.Failure<FileUploadResponse>("STORAGE_ERROR", saveResult.Error ?? "Failed to save file");

        // Create entity
        var metadata = FileMetadata.Create(request.Title, request.Description);
        var fileAttachment = FileAttachment.Create(
            originalFileName: fileName,
            storedFileName: storedFileName,
            fileExtension: extension,
            contentType: request.ContentType,
            fileSize: request.FileSize,
            storagePath: storagePath,
            category: request.Category,
            accessLevel: request.AccessLevel,
            ownerId: request.OwnerId,
            ownerType: request.OwnerType,
            ownerProperty: request.OwnerProperty,
            checksum: checksum,
            metadata: metadata);

        fileAttachment.SetCreatedBy(userId ?? _currentUserService.GetUserId());

        await _fileRepository.AddAsync(fileAttachment, cancellationToken);

        await RecordHistoryAsync(
            fileAttachment,
            FileAction.Created,
            userId,
            FileChangeDetails.CreateForUpload(),
            cancellationToken);

        return Result.Success(MapToUploadResponse(fileAttachment));
    }

    public async Task<Result<BatchUploadResponse>> UploadBatchAsync(
        UploadBatchRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var response = new BatchUploadResponse { TotalCount = request.Files.Count };

        foreach (var file in request.Files)
        {
            var uploadRequest = new UploadFileRequest
            {
                File = file,
                Category = request.Category,
                AccessLevel = request.AccessLevel,
                OwnerId = request.OwnerId,
                OwnerType = request.OwnerType,
                OwnerProperty = request.OwnerProperty
            };

            var result = await UploadAsync(uploadRequest, userId, cancellationToken);
            if (result.IsSuccess)
            {
                response.Successful.Add(result.Value);
                response.SuccessCount++;
            }
            else
            {
                response.Failed.Add(new BatchUploadError
                {
                    FileName = file.FileName,
                    Error = result.Error.Description
                });
                response.FailureCount++;
            }
        }

        return Result.Success(response);
    }

    #endregion

    #region Download Operations

    public async Task<Result<FileDownloadResponse>> DownloadAsync(
        Guid fileId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.IsDeleted)
        {
            await LogAccessAsync(fileId, FileAccessAction.NotFound, false, userId, "File not found");
            return Result.Failure<FileDownloadResponse>("FILE_NOT_FOUND", "File not found or access denied");
        }

        // Check access
        var canAccess = await CheckAccessAsync(file, userId, cancellationToken);
        if (!canAccess)
        {
            await LogAccessAsync(fileId, FileAccessAction.AccessDenied, false, userId, "Access denied");
            return Result.Failure<FileDownloadResponse>("ACCESS_DENIED", "You do not have permission to access this file");
        }

        // Get from storage
        var stream = await _storageProvider.GetAsync(file.StoragePath, cancellationToken);
        if (stream == null)
        {
            await LogAccessAsync(fileId, FileAccessAction.NotFound, false, userId, "File not found in storage");
            return Result.Failure<FileDownloadResponse>("FILE_NOT_FOUND", "File not found in storage");
        }

        await LogAccessAsync(fileId, FileAccessAction.Downloaded, true, userId);

        return Result.Success(new FileDownloadResponse
        {
            Stream = stream,
            FileName = file.OriginalFileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            ETag = file.Checksum,
            AcceptRanges = _options.Performance.EnableRangeRequests
        });
    }

    public async Task<Result<FileDownloadResponse>> DownloadStreamAsync(
        Guid fileId,
        Guid? userId = null,
        RangeItem? range = null,
        CancellationToken cancellationToken = default)
    {
        return await DownloadAsync(fileId, userId, cancellationToken);
    }

    public async Task<Result<FileAccessUrlResponse>> GetAccessUrlAsync(
        Guid fileId,
        TimeSpan? expiration = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.IsDeleted)
            return Result.Failure<FileAccessUrlResponse>("FILE_NOT_FOUND", "File not found");

        var canAccess = await CheckAccessAsync(file, userId, cancellationToken);
        if (!canAccess)
            return Result.Failure<FileAccessUrlResponse>("ACCESS_DENIED", "You do not have permission to access this file");

        var maxExpiration = TimeSpan.FromMinutes(_options.Security.MaxAccessUrlExpirationMinutes);
        var actualExpiration = expiration.HasValue
            ? (expiration.Value > maxExpiration ? maxExpiration : expiration.Value)
            : maxExpiration;

        var url = await _storageProvider.GetAccessUrlAsync(file.StoragePath, actualExpiration, cancellationToken);
        if (string.IsNullOrEmpty(url))
            return Result.Failure<FileAccessUrlResponse>("URL_GENERATION_FAILED", "Failed to generate access URL");

        return Result.Success(new FileAccessUrlResponse
        {
            FileId = fileId,
            Url = url,
            ExpiresAt = DateTime.UtcNow.Add(actualExpiration)
        });
    }

    #endregion

    #region Thumbnail & Icon Operations

    public async Task<Result<FileDownloadResponse>> GetThumbnailAsync(
        Guid fileId,
        ThumbnailSize size = ThumbnailSize.Medium,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.IsDeleted)
            return Result.Failure<FileDownloadResponse>("FILE_NOT_FOUND", "File not found");

        var canAccess = await CheckAccessAsync(file, userId, cancellationToken);
        if (!canAccess)
            return Result.Failure<FileDownloadResponse>("ACCESS_DENIED", "Access denied");

        if (string.IsNullOrEmpty(file.ThumbnailPath))
            return Result.Failure<FileDownloadResponse>("NO_THUMBNAIL", "No thumbnail available");

        var thumbnailPath = $"{file.ThumbnailPath}_{(int)size}";
        var stream = await _storageProvider.GetAsync(thumbnailPath, cancellationToken);
        if (stream == null)
            return Result.Failure<FileDownloadResponse>("THUMBNAIL_NOT_FOUND", "Thumbnail not found");

        return Result.Success(new FileDownloadResponse
        {
            Stream = stream,
            FileName = file.OriginalFileName,
            ContentType = "image/webp",
            FileSize = stream.Length,
            AcceptRanges = false
        });
    }

    public async Task<Result<FileDownloadResponse>> GetFileIconAsync(
        string extension,
        CancellationToken cancellationToken = default)
    {
        var stream = await _iconProvider.GetIconAsync(extension, cancellationToken);
        if (stream == null)
            return Result.Failure<FileDownloadResponse>("ICON_NOT_FOUND", "Icon not found");

        return Result.Success(new FileDownloadResponse
        {
            Stream = stream,
            FileName = $"icon{extension}.svg",
            ContentType = "image/svg+xml",
            FileSize = stream.Length,
            AcceptRanges = false
        });
    }

    #endregion

    #region Metadata Operations

    public async Task<Result<FileMetadataResponse>> GetMetadataAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null)
            return Result.Failure<FileMetadataResponse>("FILE_NOT_FOUND", "File not found");

        return Result.Success(MapToMetadataResponse(file));
    }

    public async Task<Result<IReadOnlyList<FileMetadataResponse>>> GetFilesByOwnerAsync(
        Guid ownerId,
        string ownerType,
        CancellationToken cancellationToken = default)
    {
        var files = await _fileRepository.GetByOwnerAsync(ownerId, ownerType, cancellationToken);
        return Result.Success((IReadOnlyList<FileMetadataResponse>)files.Select(MapToMetadataResponse).ToList());
    }

    #endregion
    #region Management Operations

    public async Task<Result<FileMetadataResponse>> UpdateAsync(
        Guid fileId,
        UpdateFileRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.IsDeleted)
            return Result.Failure<FileMetadataResponse>("FILE_NOT_FOUND", "File not found");

        var oldAccessLevel = file.AccessLevel;
        var oldOwnerId = file.OwnerId;
        var oldOwnerType = file.OwnerType;
        var oldOwnerProperty = file.OwnerProperty;

        // Update metadata
        var metadata = file.GetMetadata();
        if (request.Title != null)
            metadata = metadata.WithTitle(request.Title);
        if (request.Description != null)
            metadata = metadata.WithDescription(request.Description);
        if (request.CustomProperties != null)
        {
            foreach (var prop in request.CustomProperties)
                metadata.CustomProperties[prop.Key] = prop.Value;
        }
        file.UpdateMetadata(metadata, userId);

        // Update access level if changed
        if (request.AccessLevel.HasValue && request.AccessLevel.Value != oldAccessLevel)
        {
            file.ChangeAccessLevel(request.AccessLevel.Value, userId);
            await RecordHistoryAsync(
                file,
                FileAction.AccessLevelChanged,
                userId,
                FileChangeDetails.CreateForAccessLevelChange(oldAccessLevel.ToString(), request.AccessLevel.Value.ToString()),
                cancellationToken);
        }

        // Update ownership if changed
        if (request.OwnerId != oldOwnerId || request.OwnerType != oldOwnerType || request.OwnerProperty != oldOwnerProperty)
        {
            file.TransferOwnership(request.OwnerId, request.OwnerType, request.OwnerProperty, userId);
            await RecordHistoryAsync(
                file,
                FileAction.OwnershipTransferred,
                userId,
                FileChangeDetails.CreateForOwnershipTransfer(oldOwnerType, oldOwnerId, request.OwnerType, request.OwnerId),
                cancellationToken);
        }

        await _fileRepository.UpdateAsync(file, cancellationToken);

        return Result.Success(MapToMetadataResponse(file));
    }

    public async Task<Result<FileUploadResponse>> ReplaceAsync(
        Guid fileId,
        ReplaceFileRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var existingFile = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (existingFile == null || existingFile.IsDeleted)
            return Result.Failure<FileUploadResponse>("FILE_NOT_FOUND", "File not found");

        var file = request.File;
        var fileName = FileExtensions.SanitizeFileName(file.FileName);
        var extension = FileExtensions.GetExtension(fileName);
        var contentType = file.ContentType;

        // Validate
        using var stream = file.OpenReadStream();
        var validationResult = await _fileValidator.ValidateAsync(
            stream, fileName, contentType, file.Length, cancellationToken);

        if (!validationResult.IsValid)
        {
            var error = validationResult.Errors.First();
            return Result.Failure<FileUploadResponse>(error.Code, error.Message);
        }

        // Generate new stored filename
        var storedFileName = $"{Guid.NewGuid():N}_{Path.GetFileNameWithoutExtension(fileName)}{extension}";
        var now = DateTime.UtcNow;
        var storagePath = GenerateStoragePath(existingFile.AccessLevel, existingFile.Category, now, storedFileName);

        // Calculate checksum
        stream.Position = 0;
        var checksum = await CalculateChecksumAsync(stream);

        // Save to storage
        stream.Position = 0;
        var saveResult = await _storageProvider.SaveAsync(stream, storagePath, cancellationToken);
        if (!saveResult.Success)
            return Result.Failure<FileUploadResponse>("STORAGE_ERROR", saveResult.Error ?? "Failed to save file");

        // Create new version
        var newFile = FileAttachment.Create(
            originalFileName: fileName,
            storedFileName: storedFileName,
            fileExtension: extension,
            contentType: contentType,
            fileSize: file.Length,
            storagePath: storagePath,
            category: existingFile.Category,
            accessLevel: existingFile.AccessLevel,
            ownerId: existingFile.OwnerId,
            ownerType: existingFile.OwnerType,
            ownerProperty: existingFile.OwnerProperty,
            checksum: checksum,
            metadata: existingFile.GetMetadata());

        newFile.SetCreatedBy(userId ?? _currentUserService.GetUserId());
        newFile.SetPreviousVersion(existingFile.Id);

        // Link versions
        existingFile.SetNextVersion(newFile.Id);

        await _fileRepository.AddAsync(newFile, cancellationToken);
        await _fileRepository.UpdateAsync(existingFile, cancellationToken);

        // Record history
        await RecordHistoryAsync(
            newFile,
            FileAction.Replaced,
            userId,
            FileChangeDetails.CreateForReplacement(
                existingFile.OriginalFileName, existingFile.FileSize, existingFile.ContentType,
                fileName, file.Length, contentType).WithReason(request.ChangeReason),
            cancellationToken);

        return Result.Success(MapToUploadResponse(newFile));
    }

    public async Task<Result> DeleteAsync(
        Guid fileId,
        Guid? userId = null,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.IsDeleted)
            return Result.Failure("FILE_NOT_FOUND", "File not found");

        if (!_options.Trash.Enabled || !_options.Trash.MoveToTrashOnDelete)
        {
            // Permanent delete
            await PermanentDeleteInternalAsync(file, userId, cancellationToken);
            return Result.Success();
        }

        // Soft delete - move to trash
        var oldPath = file.StoragePath;
        var newPath = GenerateTrashPath(file.StoragePath, file.StoredFileName);

        // Move file in storage
        await _storageProvider.MoveAsync(oldPath, newPath, cancellationToken);

        // Move thumbnail if exists
        if (!string.IsNullOrEmpty(file.ThumbnailPath))
        {
            foreach (var size in new[] { 64, 150, 300 })
            {
                var oldThumbPath = $"{file.ThumbnailPath}_{size}";
                var newThumbPath = $"{newPath}_{size}";
                await _storageProvider.MoveAsync(oldThumbPath, newThumbPath, cancellationToken);
            }
        }

        // Update entity
        file.MarkAsDeleted(userId);
        await _fileRepository.UpdateAsync(file, cancellationToken);

        // Record history
        await RecordHistoryAsync(
            file,
            FileAction.Deleted,
            userId,
            FileChangeDetails.CreateForDelete(reason),
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RestoreAsync(
        Guid fileId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Trash.AllowRestoreFromTrash)
            return Result.Failure("RESTORE_DISABLED", "Restoring files from trash is disabled");

        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || !file.IsDeleted)
            return Result.Failure("FILE_NOT_FOUND", "File not found in trash");

        // Move back from trash
        var trashPath = file.StoragePath;
        var originalPath = trashPath.Replace("/trash/", $"/{(file.AccessLevel == AccessLevel.Public ? "public" : "private")}/");

        await _storageProvider.MoveAsync(trashPath, originalPath, cancellationToken);

        // Move thumbnail back
        if (!string.IsNullOrEmpty(file.ThumbnailPath))
        {
            foreach (var size in new[] { 64, 150, 300 })
            {
                var oldThumbPath = $"{file.ThumbnailPath}_{size}".Replace("/trash/", "/thumbnails/");
                var newThumbPath = $"{originalPath}_{size}";
                await _storageProvider.MoveAsync($"{file.ThumbnailPath}_{size}", newThumbPath, cancellationToken);
            }
        }

        // Restore entity
        file.Restore(userId);
        await _fileRepository.UpdateAsync(file, cancellationToken);

        // Record history
        await RecordHistoryAsync(file, FileAction.Restored, userId, null, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> PermanentDeleteAsync(
        Guid fileId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (_options.Trash.PermanentDeleteRequiresAdmin && !_currentUserService.IsAdmin())
            return Result.Failure("PERMISSION_DENIED", "Only administrators can permanently delete files");

        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null)
            return Result.Failure("FILE_NOT_FOUND", "File not found");

        await PermanentDeleteInternalAsync(file, userId, cancellationToken);
        return Result.Success();
    }

    private async Task PermanentDeleteInternalAsync(
        FileAttachment file,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        // Delete from storage
        await _storageProvider.DeleteAsync(file.StoragePath, cancellationToken);

        // Delete thumbnails
        if (!string.IsNullOrEmpty(file.ThumbnailPath))
        {
            foreach (var size in new[] { 64, 150, 300 })
            {
                await _storageProvider.DeleteAsync($"{file.ThumbnailPath}_{size}", cancellationToken);
            }
        }

        // Record history before deletion
        await RecordHistoryAsync(file, FileAction.PermanentlyDeleted, userId, null, cancellationToken);

        // Delete from database
        await _fileRepository.DeleteAsync(file, cancellationToken);
    }

    #endregion

    #region Access Control Operations

    public async Task<Result<FilePermissionResponse>> GrantAccessAsync(
        Guid fileId,
        GrantAccessRequest request,
        Guid grantedById,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.IsDeleted)
            return Result.Failure<FilePermissionResponse>("FILE_NOT_FOUND", "File not found");

        // Check if permission already exists
        var existing = await _permissionRepository.GetByFileAndUserAsync(fileId, request.UserId, cancellationToken);
        if (existing != null && !existing.IsDeleted)
        {
            // Update existing permission
            existing.ExtendExpiration(request.ExpiresAt ?? DateTime.MaxValue);
            await _permissionRepository.UpdateAsync(existing, cancellationToken);
            return Result.Success(MapToPermissionResponse(existing));
        }

        // Create new permission
        var permission = FileAccessPermission.Create(
            fileId,
            request.UserId,
            request.AccessLevel,
            grantedById,
            request.ExpiresAt,
            request.Reason);

        await _permissionRepository.AddAsync(permission, cancellationToken);

        return Result.Success(MapToPermissionResponse(permission));
    }

    public async Task<Result> RevokeAccessAsync(
        Guid fileId,
        Guid userId,
        Guid revokedById,
        CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetByFileAndUserAsync(fileId, userId, cancellationToken);
        if (permission == null || permission.IsDeleted)
            return Result.Failure("PERMISSION_NOT_FOUND", "Permission not found");

        permission.Revoke();
        await _permissionRepository.UpdateAsync(permission, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<bool>> CanAccessAsync(
        Guid fileId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.IsDeleted)
            return Result.Success(false);

        var canAccess = await CheckAccessAsync(file, userId, cancellationToken);
        return Result.Success(canAccess);
    }

    public async Task<Result<IReadOnlyList<FilePermissionResponse>>> GetPermissionsAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.GetByFileIdAsync(fileId, false, cancellationToken);
        return Result.Success((IReadOnlyList<FilePermissionResponse>)permissions.Select(MapToPermissionResponse).ToList());
    }

    #endregion

    #region History & Audit Operations

    public async Task<Result<IReadOnlyList<FileHistoryResponse>>> GetHistoryAsync(
        Guid fileId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var history = await _historyRepository.GetByFileIdAsync(fileId, skip, take, cancellationToken);
        return Result.Success((IReadOnlyList<FileHistoryResponse>)history.Select(h => new FileHistoryResponse
        {
            Id = h.Id,
            FileAttachmentId = h.FileAttachmentId,
            Action = h.Action,
            OwnerId = h.OwnerId,
            OwnerType = h.OwnerType,
            OwnerProperty = h.OwnerProperty,
            PreviousVersionId = h.PreviousVersionId,
            NewVersionId = h.NewVersionId,
            ChangeReason = h.ChangeReason,
            ChangeDetails = h.GetChangeDetails(),
            ChangedById = h.ChangedById,
            CreatedAt = h.CreatedAt
        }).ToList());
    }

    public async Task<Result<IReadOnlyList<FileAccessLogResponse>>> GetAccessLogsAsync(
        Guid fileId,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _accessLogRepository.GetByFileIdAsync(fileId, skip, take, cancellationToken);
        return Result.Success((IReadOnlyList<FileAccessLogResponse>)logs.Select(l => new FileAccessLogResponse
        {
            Id = l.Id,
            FileAttachmentId = l.FileAttachmentId,
            UserId = l.UserId,
            IpAddress = l.IpAddress,
            UserAgent = l.UserAgent,
            Action = l.Action,
            Success = l.Success,
            FailureReason = l.FailureReason,
            OwnerId = l.OwnerId,
            OwnerType = l.OwnerType,
            CreatedAt = l.CreatedAt
        }).ToList());
    }

    #endregion

    #region Search Operations

    public async Task<Result<PaginatedResult<FileMetadataResponse>>> SearchAsync(
        FileSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new FileSearchQuery
        {
            SearchTerm = request.SearchTerm,
            OwnerId = request.OwnerId,
            OwnerType = request.OwnerType,
            Category = request.Category,
            AccessLevel = request.AccessLevel,
            Bucket = request.Bucket,
            ContentType = request.ContentType,
            MinSize = request.MinSize,
            MaxSize = request.MaxSize,
            CreatedAfter = request.CreatedAfter,
            CreatedBefore = request.CreatedBefore,
            IncludeDeleted = request.IncludeDeleted,
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize,
            OrderBy = request.OrderBy,
            OrderDescending = request.OrderDescending
        };

        var files = await _fileRepository.SearchAsync(query, cancellationToken);
        var totalCount = await _fileRepository.SearchCountAsync(query, cancellationToken);

        var items = files.Select(MapToMetadataResponse).ToList();

        return Result.Success(new PaginatedResult<FileMetadataResponse>(items, totalCount, request.Page, request.PageSize));
    }

    public async Task<Result<PaginatedResult<AbandonedFileResponse>>> GetAbandonedFilesAsync(
        AbandonedFilesRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new AbandonedFilesQuery
        {
            IncludeNoOwner = request.IncludeNoOwner,
            IncludeOrphaned = request.IncludeOrphaned,
            OrphanedDays = request.OrphanedDays,
            NoAccessInDays = request.NoAccessInDays,
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize
        };

        var files = await _fileRepository.GetAbandonedAsync(query, cancellationToken);
        var totalCount = await _fileRepository.GetAbandonedCountAsync(query, cancellationToken);

        var items = files.Select(f => new AbandonedFileResponse
        {
            Id = f.Id,
            OriginalFileName = f.OriginalFileName,
            FileExtension = f.FileExtension,
            FileSize = f.FileSize,
            Category = f.Category,
            OwnerId = f.OwnerId,
            OwnerType = f.OwnerType,
            AbandonedReason = DetermineAbandonedReason(f),
            CreatedAt = f.CreatedAt,
            LastAccessedAt = null, // Would need to query access logs
            OwnerDeletedAt = f.DeletedAt
        }).ToList();

        return Result.Success(new PaginatedResult<AbandonedFileResponse>(items, totalCount, request.Page, request.PageSize));

    }

    public async Task<Result<IReadOnlyList<FileMetadataResponse>>> GetDeletedFilesAsync(
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var files = await _fileRepository.GetDeletedAsync(skip, take, cancellationToken);
        return Result.Success((IReadOnlyList<FileMetadataResponse>)files.Select(MapToMetadataResponse).ToList());
    }

    #endregion

    #region Cleanup Operations

    public async Task<Result<int>> EmptyTrashAsync(
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var deletedFiles = await _fileRepository.GetDeletedAsync(0, int.MaxValue, cancellationToken);
        var count = 0;

        foreach (var file in deletedFiles)
        {
            await PermanentDeleteInternalAsync(file, userId, cancellationToken);
            count++;
        }

        return Result.Success(count);
    }

    public async Task<Result<int>> DeleteAbandonedFilesAsync(
        AbandonedFilesRequest request,
        Guid? userId = null,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        var query = new AbandonedFilesQuery
        {
            IncludeNoOwner = request.IncludeNoOwner,
            IncludeOrphaned = request.IncludeOrphaned,
            OrphanedDays = request.OrphanedDays,
            NoAccessInDays = request.NoAccessInDays,
            Skip = 0,
            Take = int.MaxValue
        };

        var files = await _fileRepository.GetAbandonedAsync(query, cancellationToken);
        var count = 0;

        foreach (var file in files)
        {
            if (!dryRun)
            {
                await PermanentDeleteInternalAsync(file, userId, cancellationToken);
            }
            count++;
        }

        return Result.Success(count);
    }

    public async Task<Result<int>> CleanupExpiredTrashAsync(
        CancellationToken cancellationToken = default)
    {
        if (_options.Trash.AutoDeleteAfterDays <= 0)
            return Result.Success(0);

        var cutoffDate = DateTime.UtcNow.AddDays(-_options.Trash.AutoDeleteAfterDays);
        var expiredFiles = await _fileRepository.GetExpiredTrashAsync(cutoffDate, cancellationToken);
        var count = 0;

        foreach (var file in expiredFiles)
        {
            await PermanentDeleteInternalAsync(file, null, cancellationToken);
            count++;
        }

        return Result.Success(count);
    }

    #endregion

    #region Private Helper Methods

    private async Task<bool> CheckAccessAsync(
        FileAttachment file,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        // Public files - anyone can access
        if (file.AccessLevel == AccessLevel.Public)
            return true;

        // Check if user is authenticated
        if (!userId.HasValue)
        {
            if (_options.Security.RequireAuthenticationForPrivateFiles)
                return false;
            return _options.Security.AllowPublicFilesWithoutOwner && !file.HasOwner;
        }

        // Owner can always access
        if (file.OwnerId == userId)
            return true;

        // Check explicit permissions
        var hasPermission = await _permissionRepository.HasPermissionAsync(file.Id, userId.Value, cancellationToken);
        if (hasPermission)
            return true;

        // Check if user is admin
        if (_currentUserService.IsAdmin())
            return true;

        return false;
    }

    private async Task LogAccessAsync(
        Guid fileId,
        FileAccessAction action,
        bool success,
        Guid? userId = null,
        string? failureReason = null,
        string? ipAddress = null,
        string? userAgent = null,
        Guid? ownerId = null,
        string? ownerType = null)
    {
        if (!_options.Security.LogAllAccess)
        {
            // Only log failed access if configured
            if (success && !_options.Security.LogPublicFileAccess)
                return;
            if (!success && !_options.Security.LogFailedAccess)
                return;
        }

        var log = success
            ? FileAccessLog.Create(fileId, action, true, userId, ipAddress, userAgent, null, ownerId, ownerType)
            : FileAccessLog.CreateFailed(fileId, action, failureReason ?? "Unknown", userId, ipAddress, userAgent, ownerId, ownerType);

        await _accessLogRepository.AddAsync(log);
    }

    private async Task RecordHistoryAsync(
        FileAttachment file,
        FileAction action,
        Guid? userId,
        FileChangeDetails? changeDetails,
        CancellationToken cancellationToken)
    {
        if (!_options.History.Enabled)
            return;

        var history = FileHistory.Create(
            file.Id,
            action,
            file.OwnerId,
            file.OwnerType,
            file.OwnerProperty,
            userId,
            file.PreviousVersionId,
            file.NextVersionId,
            changeDetails?.ChangeReason,
            changeDetails);

        await _historyRepository.AddAsync(history, cancellationToken);
    }

    private string GenerateStoragePath(AccessLevel accessLevel, FileCategory category, DateTime date, string fileName)
    {
        var bucket = accessLevel == AccessLevel.Public ? "public" : "private";
        var categoryPath = GetCategoryPath(category);
        return $"{bucket}/{categoryPath}/{date:yyyy/MM}/{fileName}";
    }

    private string GenerateThumbnailPath(string originalPath)
    {
        return originalPath.Replace("/public/", "/thumbnails/").Replace("/private/", "/thumbnails/");
    }

    private string GenerateTrashPath(string originalPath, string fileName)
    {
        var date = DateTime.UtcNow;
        return $"trash/{date:yyyy/MM}/{fileName}";
    }

    private string GetCategoryPath(FileCategory category)
    {
        return category switch
        {
            FileCategory.ProductImage => "product-images",
            FileCategory.ProductDocument => "product-documents",
            FileCategory.CustomerAvatar => "customer-avatars",
            FileCategory.CustomerDocument => "customer-documents",
            FileCategory.MaintenanceAttachment => "maintenance-attachments",
            FileCategory.MaintenanceImage => "maintenance-images",
            FileCategory.GeneralDocument => "general-documents",
            FileCategory.GeneralImage => "general-images",
            _ => "other"
        };
    }

    private string DetermineAbandonedReason(FileAttachment file)
    {
        if (!file.OwnerId.HasValue && string.IsNullOrEmpty(file.OwnerType))
            return "No owner assigned";
        if (file.IsDeleted)
            return "Owner entity is deleted";
        return "Unknown";
    }

    private async Task<string> CalculateChecksumAsync(Stream stream)
    {
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hash);
    }

    private FileUploadResponse MapToUploadResponse(FileAttachment file)
    {
        return new FileUploadResponse
        {
            Id = file.Id,
            OriginalFileName = file.OriginalFileName,
            StoredFileName = file.StoredFileName,
            FileExtension = file.FileExtension,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            FileSizeFormatted = FileExtensions.FormatFileSize(file.FileSize),
            Category = file.Category,
            AccessLevel = file.AccessLevel,
            OwnerId = file.OwnerId,
            OwnerType = file.OwnerType,
            OwnerProperty = file.OwnerProperty,
            ThumbnailUrl = !string.IsNullOrEmpty(file.ThumbnailPath) ? $"/api/files/{file.Id}/thumbnail" : null,
            DownloadUrl = $"/api/files/{file.Id}",
            CreatedAt = file.CreatedAt
        };
    }

    private FileMetadataResponse MapToMetadataResponse(FileAttachment file)
    {
        return new FileMetadataResponse
        {
            Id = file.Id,
            OriginalFileName = file.OriginalFileName,
            StoredFileName = file.StoredFileName,
            FileExtension = file.FileExtension,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            FileSizeFormatted = FileExtensions.FormatFileSize(file.FileSize),
            Category = file.Category,
            AccessLevel = file.AccessLevel,
            Bucket = file.Bucket,
            OwnerId = file.OwnerId,
            OwnerType = file.OwnerType,
            OwnerProperty = file.OwnerProperty,
            ThumbnailUrl = !string.IsNullOrEmpty(file.ThumbnailPath) ? $"/api/files/{file.Id}/thumbnail" : null,
            Checksum = file.Checksum,
            Metadata = file.GetMetadata(),
            IsDeleted = file.IsDeleted,
            DeletedAt = file.DeletedAt,
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt
        };
    }

    private FilePermissionResponse MapToPermissionResponse(FileAccessPermission permission)
    {
        return new FilePermissionResponse
        {
            Id = permission.Id,
            FileAttachmentId = permission.FileAttachmentId,
            UserId = permission.UserId,
            AccessLevel = permission.AccessLevel,
            GrantedById = permission.GrantedById,
            GrantedAt = permission.GrantedAt,
            ExpiresAt = permission.ExpiresAt,
            Reason = permission.Reason,
            IsValid = permission.IsValid
        };
    }

    #endregion
}
