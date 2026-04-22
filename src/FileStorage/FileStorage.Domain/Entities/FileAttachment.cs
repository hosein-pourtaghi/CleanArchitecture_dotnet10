// FileStorage.Domain/Entities/FileAttachment.cs
using SharedKernel.BaseEntities;
using FileStorage.Domain.Enums;
using FileStorage.Domain.ValueObjects;

namespace FileStorage.Domain.Entities;

/// <summary>
/// Core entity representing a file attachment.
/// Extends Entity base class for soft delete, timestamps, and domain events.
/// </summary>
public class FileAttachment : Entity
{
    #region Properties

    /// <summary>
    /// User's original filename when uploaded.
    /// </summary>
    public string OriginalFileName { get; private set; } = string.Empty;

    /// <summary>
    /// Generated unique filename on storage.
    /// </summary>
    public string StoredFileName { get; private set; } = string.Empty;

    /// <summary>
    /// File extension including dot (e.g., ".pdf", ".jpg").
    /// </summary>
    public string FileExtension { get; private set; } = string.Empty;

    /// <summary>
    /// MIME type (e.g., "application/pdf", "image/jpeg").
    /// </summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    /// Physical/virtual path in storage.
    /// </summary>
    public string StoragePath { get; private set; } = string.Empty;

    /// <summary>
    /// Category for organizing files.
    /// </summary>
    public FileCategory Category { get; private set; }

    /// <summary>
    /// Access level for the file.
    /// </summary>
    public AccessLevel AccessLevel { get; private set; }

    /// <summary>
    /// ID of the entity that owns this file.
    /// </summary>
    public Guid? OwnerId { get; private set; }

    /// <summary>
    /// Type of entity that owns this file (e.g., "Product", "Customer").
    /// </summary>
    public string? OwnerType { get; private set; }

    /// <summary>
    /// Property name on the owner entity (e.g., "ProfileImage", "Attachment").
    /// </summary>
    public string? OwnerProperty { get; private set; }

    /// <summary>
    /// Storage bucket (public/private/trash).
    /// </summary>
    public FileBucket Bucket { get; private set; }

    /// <summary>
    /// Path to generated thumbnail.
    /// </summary>
    public string? ThumbnailPath { get; private set; }

    /// <summary>
    /// Additional metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; private set; }

    /// <summary>
    /// File hash for integrity verification.
    /// </summary>
    public string? Checksum { get; private set; }

    /// <summary>
    /// ID of the previous version (for version tracking).
    /// </summary>
    public Guid? PreviousVersionId { get; private set; }

    /// <summary>
    /// ID of the next version (for version tracking).
    /// </summary>
    public Guid? NextVersionId { get; private set; }

    #endregion

    #region Navigation Properties

    private readonly List<FileHistory> _history = new();
    public IReadOnlyCollection<FileHistory> History => _history.AsReadOnly();

    private readonly List<FileAccessLog> _accessLogs = new();
    public IReadOnlyCollection<FileAccessLog> AccessLogs => _accessLogs.AsReadOnly();

    private readonly List<FileAccessPermission> _permissions = new();
    public IReadOnlyCollection<FileAccessPermission> Permissions => _permissions.AsReadOnly();

    #endregion

    #region Constructors

    /// <summary>
    /// For ORM deserialization
    /// </summary>
    protected FileAttachment() { }

    /// <summary>
    /// Factory method for creating new file attachments.
    /// </summary>
    public static FileAttachment Create(
        string originalFileName,
        string storedFileName,
        string fileExtension,
        string contentType,
        long fileSize,
        string storagePath,
        FileCategory category,
        AccessLevel accessLevel,
        Guid? ownerId = null,
        string? ownerType = null,
        string? ownerProperty = null,
        string? checksum = null,
        FileMetadata? metadata = null)
    {
        var file = new FileAttachment
        {
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            FileExtension = fileExtension.ToLowerInvariant(),
            ContentType = contentType,
            FileSize = fileSize,
            StoragePath = storagePath,
            Category = category,
            AccessLevel = accessLevel,
            OwnerId = ownerId,
            OwnerType = ownerType,
            OwnerProperty = ownerProperty,
            Bucket = accessLevel == AccessLevel.Public ? FileBucket.Public : FileBucket.Private,
            Checksum = checksum,
            MetadataJson = metadata?.ToJson()
        };

        file.AddDomainEvent(new Events.FileUploadedEvent(file));
        return file;
    }

    #endregion

    #region Business Methods

    /// <summary>
    /// Update metadata (title, description, etc.).
    /// </summary>
    public void UpdateMetadata(FileMetadata metadata, Guid? updatedById = null)
    {
        MetadataJson = metadata.ToJson();
        SetUpdatedBy(updatedById);
        AddDomainEvent(new Events.FileMetadataUpdatedEvent(Id));
    }

    /// <summary>
    /// Change the access level of the file.
    /// </summary>
    public void ChangeAccessLevel(AccessLevel newLevel, Guid? updatedById = null)
    {
        var oldLevel = AccessLevel;
        AccessLevel = newLevel;
        Bucket = newLevel == AccessLevel.Public ? FileBucket.Public : FileBucket.Private;
        SetUpdatedBy(updatedById);
        AddDomainEvent(new Events.FileAccessLevelChangedEvent(Id, oldLevel, newLevel));
    }

    /// <summary>
    /// Transfer ownership to a different entity.
    /// </summary>
    public void TransferOwnership(Guid? newOwnerId, string? newOwnerType, string? newOwnerProperty, Guid? updatedById = null)
    {
        OwnerId = newOwnerId;
        OwnerType = newOwnerType;
        OwnerProperty = newOwnerProperty;
        SetUpdatedBy(updatedById);
        AddDomainEvent(new Events.FileOwnershipTransferredEvent(Id, newOwnerId, newOwnerType));
    }

    /// <summary>
    /// Mark file as deleted (soft delete - moves to trash).
    /// </summary>
    public void MarkAsDeleted(Guid? deletedById = null)
    {
        SoftDelete(deletedById);
        Bucket = FileBucket.Trash;
        AddDomainEvent(new Events.FileDeletedEvent(Id));
    }

    /// <summary>
    /// Restore file from trash.
    /// </summary>
    public void Restore(Guid? restoredById = null)
    {
        Restore();
        Bucket = AccessLevel == AccessLevel.Public ? FileBucket.Public : FileBucket.Private;
        AddDomainEvent(new Events.FileRestoredEvent(Id));
    }

    /// <summary>
    /// Set thumbnail path.
    /// </summary>
    public void SetThumbnailPath(string thumbnailPath)
    {
        ThumbnailPath = thumbnailPath;
    }

    /// <summary>
    /// Link to previous version.
    /// </summary>
    public void SetPreviousVersion(Guid? previousVersionId)
    {
        PreviousVersionId = previousVersionId;
    }

    /// <summary>
    /// Link to next version.
    /// </summary>
    public void SetNextVersion(Guid? nextVersionId)
    {
        NextVersionId = nextVersionId;
    }

    /// <summary>
    /// Get metadata as value object.
    /// </summary>
    public FileMetadata GetMetadata() => FileMetadata.FromJson(MetadataJson);

    /// <summary>
    /// Check if file is in trash.
    /// </summary>
    public bool IsInTrash => Bucket == FileBucket.Trash;

    /// <summary>
    /// Check if file has an owner.
    /// </summary>
    public bool HasOwner => OwnerId.HasValue && !string.IsNullOrEmpty(OwnerType);

    /// <summary>
    /// Check if this is an image file.
    /// </summary>
    public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Check if this is a document file.
    /// </summary>
    public bool IsDocument => ContentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase) ||
                              ContentType.Contains("document") ||
                              ContentType.Contains("msword") ||
                              ContentType.Contains("excel") ||
                              ContentType.Contains("spreadsheet");

    #endregion
}
