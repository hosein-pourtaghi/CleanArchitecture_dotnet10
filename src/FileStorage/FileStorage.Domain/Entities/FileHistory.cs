// FileStorage.Domain/Entities/FileHistory.cs
using SharedKernel.BaseEntities;
using FileStorage.Domain.Enums;
using FileStorage.Domain.ValueObjects;

namespace FileStorage.Domain.Entities;

/// <summary>
/// Tracks every change to files with entity ownership information.
/// Critical for answering "What files did this deleted customer have?"
/// </summary>
public class FileHistory : Entity
{
    #region Properties

    /// <summary>
    /// Link to the file attachment.
    /// </summary>
    public Guid FileAttachmentId { get; private set; }

    /// <summary>
    /// Action performed on the file.
    /// </summary>
    public FileAction Action { get; private set; }

    /// <summary>
    /// Ownership at time of action (critical for tracking).
    /// </summary>
    public Guid? OwnerId { get; private set; }

    /// <summary>
    /// Entity type at time of action.
    /// </summary>
    public string? OwnerType { get; private set; }

    /// <summary>
    /// Property name at time of action.
    /// </summary>
    public string? OwnerProperty { get; private set; }

    /// <summary>
    /// For version tracking - previous version ID.
    /// </summary>
    public Guid? PreviousVersionId { get; private set; }

    /// <summary>
    /// For version tracking - new version ID.
    /// </summary>
    public Guid? NewVersionId { get; private set; }

    /// <summary>
    /// Optional reason for change.
    /// </summary>
    public string? ChangeReason { get; private set; }

    /// <summary>
    /// JSON with specific property changes.
    /// </summary>
    public string? ChangeDetailsJson { get; private set; }

    /// <summary>
    /// Who made the change.
    /// </summary>
    public Guid? ChangedById { get; private set; }

    /// <summary>
    /// IP address of the user who made the change.
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// User agent of the client.
    /// </summary>
    public string? UserAgent { get; private set; }

    #endregion

    #region Navigation Properties

    private FileAttachment? _fileAttachment;
    public FileAttachment? FileAttachment => _fileAttachment;

    #endregion

    #region Constructors

    /// <summary>
    /// For ORM deserialization
    /// </summary>
    protected FileHistory() { }

    /// <summary>
    /// Create a history record.
    /// </summary>
    public static FileHistory Create(
        Guid fileAttachmentId,
        FileAction action,
        Guid? ownerId,
        string? ownerType,
        string? ownerProperty,
        Guid? changedById,
        Guid? previousVersionId = null,
        Guid? newVersionId = null,
        string? changeReason = null,
        FileChangeDetails? changeDetails = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new FileHistory
        {
            FileAttachmentId = fileAttachmentId,
            Action = action,
            OwnerId = ownerId,
            OwnerType = ownerType,
            OwnerProperty = ownerProperty,
            PreviousVersionId = previousVersionId,
            NewVersionId = newVersionId,
            ChangeReason = changeReason,
            ChangeDetailsJson = changeDetails?.ToJson(),
            ChangedById = changedById,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
    }

    #endregion

    #region Business Methods

    /// <summary>
    /// Get change details as value object.
    /// </summary>
    public FileChangeDetails? GetChangeDetails()
    {
        if (string.IsNullOrEmpty(ChangeDetailsJson))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<FileChangeDetails>(ChangeDetailsJson);
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
