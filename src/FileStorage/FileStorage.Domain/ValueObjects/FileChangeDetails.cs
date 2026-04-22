// FileStorage.Domain/ValueObjects/FileChangeDetails.cs
using System.Text.Json;

namespace FileStorage.Domain.ValueObjects;

/// <summary>
/// Value object for tracking file change details in history.
/// </summary>
public sealed class FileChangeDetails
{
    public string? OldFileName { get; private set; }
    public string? NewFileName { get; private set; }
    public long? OldFileSize { get; private set; }
    public long? NewFileSize { get; private set; }
    public string? OldContentType { get; private set; }
    public string? NewContentType { get; private set; }
    public string? OldAccessLevel { get; private set; }
    public string? NewAccessLevel { get; private set; }
    public string? OldOwnerType { get; private set; }
    public string? OldOwnerId { get; private set; }
    public string? NewOwnerType { get; private set; }
    public string? NewOwnerId { get; private set; }
    public string? ChangeReason { get; private set; }
    public Dictionary<string, string> AdditionalChanges { get; private set; } = new();

    private FileChangeDetails() { }

    public static FileChangeDetails CreateForUpload()
    {
        return new FileChangeDetails();
    }

    public static FileChangeDetails CreateForReplacement(
        string oldFileName, long oldFileSize, string oldContentType,
        string newFileName, long newFileSize, string newContentType)
    {
        return new FileChangeDetails
        {
            OldFileName = oldFileName,
            NewFileName = newFileName,
            OldFileSize = oldFileSize,
            NewFileSize = newFileSize,
            OldContentType = oldContentType,
            NewContentType = newContentType
        };
    }

    public static FileChangeDetails CreateForAccessLevelChange(
        string oldLevel, string newLevel)
    {
        return new FileChangeDetails
        {
            OldAccessLevel = oldLevel,
            NewAccessLevel = newLevel
        };
    }

    public static FileChangeDetails CreateForOwnershipTransfer(
        string oldOwnerType, Guid? oldOwnerId,
        string newOwnerType, Guid? newOwnerId)
    {
        return new FileChangeDetails
        {
            OldOwnerType = oldOwnerType,
            OldOwnerId = oldOwnerId?.ToString(),
            NewOwnerType = newOwnerType,
            NewOwnerId = newOwnerId?.ToString()
        };
    }

    public static FileChangeDetails CreateForDelete(string? reason = null)
    {
        return new FileChangeDetails { ChangeReason = reason };
    }

    public FileChangeDetails WithReason(string reason)
    {
        return new FileChangeDetails
        {
            OldFileName = OldFileName,
            NewFileName = NewFileName,
            OldFileSize = OldFileSize,
            NewFileSize = NewFileSize,
            OldContentType = OldContentType,
            NewContentType = NewContentType,
            OldAccessLevel = OldAccessLevel,
            NewAccessLevel = NewAccessLevel,
            OldOwnerType = OldOwnerType,
            OldOwnerId = OldOwnerId,
            NewOwnerType = NewOwnerType,
            NewOwnerId = NewOwnerId,
            ChangeReason = reason,
            AdditionalChanges = AdditionalChanges
        };
    }

    public string ToJson()
    {
        var dict = new Dictionary<string, object?>();

        if (OldFileName != null)
            dict["oldFileName"] = OldFileName;
        if (NewFileName != null)
            dict["newFileName"] = NewFileName;
        if (OldFileSize.HasValue)
            dict["oldFileSize"] = OldFileSize.Value;
        if (NewFileSize.HasValue)
            dict["newFileSize"] = NewFileSize.Value;
        if (OldContentType != null)
            dict["oldContentType"] = OldContentType;
        if (NewContentType != null)
            dict["newContentType"] = NewContentType;
        if (OldAccessLevel != null)
            dict["oldAccessLevel"] = OldAccessLevel;
        if (NewAccessLevel != null)
            dict["newAccessLevel"] = NewAccessLevel;
        if (OldOwnerType != null)
            dict["oldOwnerType"] = OldOwnerType;
        if (OldOwnerId != null)
            dict["oldOwnerId"] = OldOwnerId;
        if (NewOwnerType != null)
            dict["newOwnerType"] = NewOwnerType;
        if (NewOwnerId != null)
            dict["newOwnerId"] = NewOwnerId;
        if (ChangeReason != null)
            dict["changeReason"] = ChangeReason;

        foreach (var kvp in AdditionalChanges)
            dict[kvp.Key] = kvp.Value;

        return JsonSerializer.Serialize(dict);
    }
}
