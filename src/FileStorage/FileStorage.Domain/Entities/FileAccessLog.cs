// FileStorage.Domain/Entities/FileAccessLog.cs
using SharedKernel.BaseEntities;
using FileStorage.Domain.Enums;

namespace FileStorage.Domain.Entities;

/// <summary>
/// Logs every access attempt for security audit.
/// </summary>
public class FileAccessLog : Entity
{
    #region Properties

    /// <summary>
    /// Which file was accessed.
    /// </summary>
    public Guid FileAttachmentId { get; private set; }

    /// <summary>
    /// Who tried to access (null for public access).
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Client IP address.
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// Browser/client info.
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Action performed (Viewed, Downloaded, etc.).
    /// </summary>
    public FileAccessAction Action { get; private set; }

    /// <summary>
    /// Whether access was granted.
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Why access was denied.
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// Ownership at time of access.
    /// </summary>
    public Guid? OwnerId { get; private set; }

    /// <summary>
    /// Entity type at time of access.
    /// </summary>
    public string? OwnerType { get; private set; }

    /// <summary>
    /// Request path.
    /// </summary>
    public string? RequestPath { get; private set; }

    /// <summary>
    /// HTTP method.
    /// </summary>
    public string? HttpMethod { get; private set; }

    /// <summary>
    /// Response status code.
    /// </summary>
    public int? StatusCode { get; private set; }

    /// <summary>
    /// Response time in milliseconds.
    /// </summary>
    public long? ResponseTimeMs { get; private set; }

    #endregion

    #region Navigation Properties

    private FileAttachment? _fileAttachment;
    public FileAttachment? FileAttachment => _fileAttachment;

    #endregion

    #region Constructors

    /// <summary>
    /// For ORM deserialization
    /// </summary>
    protected FileAccessLog() { }

    /// <summary>
    /// Create an access log entry.
    /// </summary>
    public static FileAccessLog Create(
        Guid fileAttachmentId,
        FileAccessAction action,
        bool success,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? failureReason = null,
        Guid? ownerId = null,
        string? ownerType = null,
        string? requestPath = null,
        string? httpMethod = null,
        int? statusCode = null,
        long? responseTimeMs = null)
    {
        return new FileAccessLog
        {
            FileAttachmentId = fileAttachmentId,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Action = action,
            Success = success,
            FailureReason = failureReason,
            OwnerId = ownerId,
            OwnerType = ownerType,
            RequestPath = requestPath,
            HttpMethod = httpMethod,
            StatusCode = statusCode,
            ResponseTimeMs = responseTimeMs
        };
    }

    /// <summary>
    /// Create a failed access log entry.
    /// </summary>
    public static FileAccessLog CreateFailed(
        Guid fileAttachmentId,
        FileAccessAction action,
        string failureReason,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        Guid? ownerId = null,
        string? ownerType = null)
    {
        return Create(
            fileAttachmentId, action, false, userId, ipAddress, userAgent,
            failureReason, ownerId, ownerType);
    }

    /// <summary>
    /// Create a not found log entry.
    /// </summary>
    public static FileAccessLog CreateNotFound(
        Guid fileAttachmentId,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return Create(
            fileAttachmentId, FileAccessAction.NotFound, false, userId, ipAddress, userAgent,
            "File not found or access denied");
    }

    #endregion
}
