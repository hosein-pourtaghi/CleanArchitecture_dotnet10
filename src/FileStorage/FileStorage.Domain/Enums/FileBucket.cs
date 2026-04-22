// FileStorage.Domain/Enums/FileBucket.cs
namespace FileStorage.Domain.Enums;

/// <summary>
/// Storage buckets for organizing files by status.
/// </summary>
public enum FileBucket
{
    /// <summary>
    /// Public accessible files
    /// </summary>
    Public = 0,

    /// <summary>
    /// Private/protected files
    /// </summary>
    Private = 1,

    /// <summary>
    /// Soft-deleted files (trash)
    /// </summary>
    Trash = 2,

    /// <summary>
    /// Temporary upload staging
    /// </summary>
    Temp = 3
}
