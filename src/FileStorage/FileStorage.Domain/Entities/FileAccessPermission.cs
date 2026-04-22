// FileStorage.Domain/Entities/FileAccessPermission.cs
using SharedKernel.BaseEntities;
using FileStorage.Domain.Enums;

namespace FileStorage.Domain.Entities;

/// <summary>
/// Explicit permissions for private files.
/// </summary>
public class FileAccessPermission : Entity
{
    #region Properties

    /// <summary>
    /// Which file this permission is for.
    /// </summary>
    public Guid FileAttachmentId { get; private set; }

    /// <summary>
    /// Who has access.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Level of access granted.
    /// </summary>
    public AccessLevel AccessLevel { get; private set; }

    /// <summary>
    /// Who granted this permission.
    /// </summary>
    public Guid? GrantedById { get; private set; }

    /// <summary>
    /// When the permission was granted.
    /// </summary>
    public DateTime GrantedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional expiration.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// Optional reason/notes for the permission.
    /// </summary>
    public string? Reason { get; private set; }

    #endregion

    #region Navigation Properties

    private FileAttachment? _fileAttachment;
    public FileAttachment? FileAttachment => _fileAttachment;

    #endregion

    #region Constructors

    /// <summary>
    /// For ORM deserialization
    /// </summary>
    protected FileAccessPermission() { }

    /// <summary>
    /// Create a new permission.
    /// </summary>
    public static FileAccessPermission Create(
        Guid fileAttachmentId,
        Guid userId,
        AccessLevel accessLevel,
        Guid? grantedById,
        DateTime? expiresAt = null,
        string? reason = null)
    {
        return new FileAccessPermission
        {
            FileAttachmentId = fileAttachmentId,
            UserId = userId,
            AccessLevel = accessLevel,
            GrantedById = grantedById,
            ExpiresAt = expiresAt,
            Reason = reason
        };
    }

    #endregion

    #region Business Methods

    /// <summary>
    /// Check if permission is still valid.
    /// </summary>
    public bool IsValid => !ExpiresAt.HasValue || ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Check if permission has expired.
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt <= DateTime.UtcNow;

    /// <summary>
    /// Revoke this permission.
    /// </summary>
    public void Revoke()
    {
        SoftDelete(null);
    }

    /// <summary>
    /// Extend the expiration.
    /// </summary>
    public void ExtendExpiration(DateTime newExpiration)
    {
        ExpiresAt = newExpiration;
    }

    #endregion
}
