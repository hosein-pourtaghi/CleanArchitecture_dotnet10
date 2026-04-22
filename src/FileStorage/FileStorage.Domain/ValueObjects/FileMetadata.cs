// FileStorage.Domain/ValueObjects/FileMetadata.cs
using System.Text.Json;

namespace FileStorage.Domain.ValueObjects;

/// <summary>
/// Value object containing additional file metadata.
/// </summary>
public sealed class FileMetadata
{
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public int? Duration { get; private set; } // For videos/audio in seconds
    public Dictionary<string, string> CustomProperties { get; private set; } = new();

    public FileMetadata() { }

    public static FileMetadata Create(
        string? title = null,
        string? description = null,
        int? width = null,
        int? height = null,
        int? duration = null,
        Dictionary<string, string>? customProperties = null)
    {
        return new FileMetadata
        {
            Title = title,
            Description = description,
            Width = width,
            Height = height,
            Duration = duration,
            CustomProperties = customProperties ?? new Dictionary<string, string>()
        };
    }

    public static FileMetadata FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Create();

        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            // todo: refactor all props for null check :  title: dict?.GetValueOrDefault("title")?.GetString(),
            return Create(
                title: dict?.GetValueOrDefault("title").GetString(),
                description: dict?.GetValueOrDefault("description").GetString(),
                width: dict?.GetValueOrDefault("width").GetInt32(),
                height: dict?.GetValueOrDefault("height").GetInt32(),
                duration: dict?.GetValueOrDefault("duration").GetInt32(),
                customProperties: dict?.Where(kv => !new[] { "title", "description", "width", "height", "duration" }.Contains(kv.Key))
                    .ToDictionary(kv => kv.Key, kv => kv.Value.ToString())
            );
        }
        catch
        {
            return Create();
        }
    }

    public string ToJson()
    {
        var dict = new Dictionary<string, object?>();

        if (Title != null)
            dict["title"] = Title;
        if (Description != null)
            dict["description"] = Description;
        if (Width.HasValue)
            dict["width"] = Width.Value;
        if (Height.HasValue)
            dict["height"] = Height.Value;
        if (Duration.HasValue)
            dict["duration"] = Duration.Value;

        foreach (var kvp in CustomProperties)
            dict[kvp.Key] = kvp.Value;

        return JsonSerializer.Serialize(dict);
    }

    public FileMetadata WithTitle(string title) => Create(title, Description, Width, Height, Duration, CustomProperties);
    public FileMetadata WithDescription(string description) => Create(Title, description, Width, Height, Duration, CustomProperties);
    public FileMetadata WithDimensions(int width, int height) => Create(Title, Description, width, height, Duration, CustomProperties);
    public FileMetadata WithDuration(int duration) => Create(Title, Description, Width, Height, duration, CustomProperties);
}
