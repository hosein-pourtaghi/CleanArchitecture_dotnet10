// =====================================================================
// FileService - Handles file operations for generated code
// =====================================================================

using Microsoft.Extensions.Logging;

namespace CRUDGenerator.Services;

/// <summary>
/// Interface for file operations
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Find entity files in the input directory
    /// </summary>
    List<string> FindEntityFiles(string inputDirectory, string? entityName = null);

    /// <summary>
    /// Save generated file to disk
    /// </summary>
    Task SaveFileAsync(string filePath, string content, bool overwrite = false);
}

/// <summary>
/// Handles file operations for generated code
/// </summary>
public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
    }

    public List<string> FindEntityFiles(string inputDirectory, string? entityName = null)
    {
        var entityFiles = new List<string>();

        try
        {
            // Search for .cs files in the input directory and subdirectories
            var searchOption = SearchOption.AllDirectories;
            var files = Directory.GetFiles(inputDirectory, "*.cs", searchOption);

            foreach (var file in files)
            {
                // Skip non-entity files
                if (IsExcludedFile(file))
                    continue;

                // If entity name is specified, only include that entity
                if (!string.IsNullOrEmpty(entityName))
                {
                    if (Path.GetFileNameWithoutExtension(file)
                        .Equals(entityName, StringComparison.OrdinalIgnoreCase))
                    {
                        entityFiles.Add(file);
                    }
                }
                else
                {
                    // Check if the file contains an entity class
                    if (IsEntityFile(file))
                    {
                        entityFiles.Add(file);
                    }
                }
            }

            _logger.LogDebug("Found {Count} entity file(s) in {Directory}",
                entityFiles.Count, inputDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for entity files in {Directory}", inputDirectory);
        }

        return entityFiles;
    }

    public async Task SaveFileAsync(string filePath, string content, bool overwrite = false)
    {
        try
        {
            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Check if file exists
            if (File.Exists(filePath) && !overwrite)
            {
                _logger.LogWarning("File already exists and overwrite is disabled: {FilePath}", filePath);
                return;
            }

            // Write file
            await File.WriteAllTextAsync(filePath, content);
            _logger.LogDebug("Saved file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FilePath}", filePath);
            throw;
        }
    }

    private static bool IsExcludedFile(string filePath)
    {
        var excludedPatterns = new[]
        {
            "AssemblyInfo.cs",
            "GlobalUsings.cs",
            "Program.cs",
            ".Designer.cs",
            ".generated.cs",
            "TemporaryGeneratedFile"
        };

        var fileName = Path.GetFileName(filePath);
        return excludedPatterns.Any(pattern =>
            fileName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsEntityFile(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);

            // Check for common entity patterns
            var entityPatterns = new[]
            {
                @":\s*Entity\b",
                @":\s*AggregateRoot\b",
                @":\s*BaseEntity\b",
                @":\s*AuditableEntity\b",
                @"public\s+class\s+\w+\s*:\s*\w*Entity",
                @"public\s+class\s+\w+\s*:\s*\w*Aggregate"
            };

            return entityPatterns.Any(pattern =>
                System.Text.RegularExpressions.Regex.IsMatch(content, pattern));
        }
        catch
        {
            return false;
        }
    }
}
