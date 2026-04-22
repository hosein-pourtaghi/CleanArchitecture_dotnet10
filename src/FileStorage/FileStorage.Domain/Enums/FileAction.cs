// FileStorage.Domain/Enums/FileAction.cs
namespace FileStorage.Domain.Enums;

/// <summary>
/// Actions that can be performed on files (for history tracking).
/// </summary>
public enum FileAction
{
    Created = 0,
    Updated = 1,
    Replaced = 2,
    Deleted = 3,
    Restored = 4,
    PermanentlyDeleted = 5,
    AccessLevelChanged = 6,
    OwnershipTransferred = 7,
    MetadataUpdated = 8
}
