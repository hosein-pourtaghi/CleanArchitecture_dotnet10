// FileStorage.Domain/Enums/FileAccessAction.cs
namespace FileStorage.Domain.Enums;

/// <summary>
/// Actions for access logging (security audit).
/// </summary>
public enum FileAccessAction
{
    Viewed = 0,
    Downloaded = 1,
    Uploaded = 2,
    Deleted = 3,
    AccessDenied = 4,
    NotFound = 5
}
