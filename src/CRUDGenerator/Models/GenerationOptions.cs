// =====================================================================
// GenerationOptions - Configuration for CRUD code generation
// =====================================================================

namespace CRUDGenerator.Models;

/// <summary>
/// Configuration options for CRUD code generation
/// </summary>
public class GenerationOptions
{
    /// <summary>
    /// Directory containing entity files to process
    /// </summary>
    public string InputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Output directory for generated files
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Specific entity name to generate (null = all entities)
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Base namespace for generated code
    /// </summary>
    public string BaseNamespace { get; set; } = "YourApp";

    /// <summary>
    /// Generate all code in a single file per entity
    /// </summary>
    public bool GenerateSingleFile { get; set; } = true;

    /// <summary>
    /// Overwrite existing files
    /// </summary>
    public bool OverwriteExisting { get; set; } = false;

    /// <summary>
    /// Include FluentValidation rules
    /// </summary>
    public bool IncludeValidation { get; set; } = true;

    /// <summary>
    /// Include domain event handlers
    /// </summary>
    public bool IncludeDomainEvents { get; set; } = true;

    /// <summary>
    /// Include repository interface
    /// </summary>
    public bool IncludeRepository { get; set; } = true;
}
