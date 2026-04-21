// =====================================================================
// EntityInfo - Represents a parsed entity with all its metadata
// =====================================================================

namespace CRUDGenerator.Models;

/// <summary>
/// Represents a parsed entity with all its metadata
/// </summary>
public class EntityInfo
{
    /// <summary>
    /// Entity name (class name)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full namespace of the entity
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Base class if any (e.g., Entity, AggregateRoot)
    /// </summary>
    public string? BaseClass { get; set; }

    /// <summary>
    /// List of properties in the entity
    /// </summary>
    public List<EntityProperty> Properties { get; set; } = new();

    /// <summary>
    /// Navigation properties (relationships)
    /// </summary>
    public List<NavigationProperty> NavigationProperties { get; set; } = new();

    /// <summary>
    /// File path where the entity was found
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is an aggregate root
    /// </summary>
    public bool IsAggregateRoot => BaseClass?.Contains("AggregateRoot") == true;

    /// <summary>
    /// Whether this entity inherits from Entity base class
    /// </summary>
    public bool IsEntity => BaseClass?.Contains("Entity") == true;

    /// <summary>
    /// Get properties that should be in Create DTO (exclude Id, timestamps)
    /// </summary>
    public IEnumerable<EntityProperty> CreateProperties =>
        Properties.Where(p => !IsSystemProperty(p.Name) && !IsNavigationProperty(p.Name));

    /// <summary>
    /// Get properties that should be in Update DTO (include Id)
    /// </summary>
    public IEnumerable<EntityProperty> UpdateProperties =>
        Properties.Where(p => !IsSystemProperty(p.Name) && !IsNavigationProperty(p.Name));

    /// <summary>
    /// Get properties that should be in Response DTO (all except navigation)
    /// </summary>
    public IEnumerable<EntityProperty> ResponseProperties =>
        Properties.Where(p => !IsNavigationProperty(p.Name));

    private static bool IsSystemProperty(string name)
    {
        var systemProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy",
            "RowVersion", "ConcurrencyToken", "DomainEvents"
        };
        return systemProps.Contains(name);
    }

    private bool IsNavigationProperty(string name)
    {
        return NavigationProperties.Any(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Represents a property in an entity
/// </summary>
public class EntityProperty
{
    /// <summary>
    /// Property name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Property type (e.g., string, Guid, int)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Is the property nullable?
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Is this a value type (struct)?
    /// </summary>
    public bool IsValueType { get; set; }

    /// <summary>
    /// Is this a collection property?
    /// </summary>
    public bool IsCollection { get; set; }

    /// <summary>
    /// Is this property required?
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Is this the Id property?
    /// </summary>
    public bool IsId => Name.Equals("Id", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Maximum length for string properties
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Is this an enum?
    /// </summary>
    public bool IsEnum { get; set; }

    /// <summary>
    /// Get the C# type with nullable annotation
    /// </summary>
    public string GetTypeWithAnnotation()
    {
        if (IsCollection)
            return Type;

        if (IsNullable && !Type.EndsWith("?"))
            return $"{Type}?";

        return Type;
    }

    /// <summary>
    /// Get the type for DTO (handles collections)
    /// </summary>
    public string GetDtoType()
    {
        if (IsCollection)
        {
            // Extract element type from collection
            var elementType = ExtractElementType(Type);
            return $"ICollection<{elementType}Dto>";
        }
        return Type;
    }

    private static string ExtractElementType(string collectionType)
    {
        // Handle List<T>, ICollection<T>, IEnumerable<T>
        var match = System.Text.RegularExpressions.Regex.Match(collectionType, @"(\w+)<(\w+)>");
        if (match.Success)
            return match.Groups[2].Value;
        return "object";
    }
}

/// <summary>
/// Represents a navigation property (relationship)
/// </summary>
public class NavigationProperty
{
    /// <summary>
    /// Property name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of the navigation property
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Is this a collection navigation?
    /// </summary>
    public bool IsCollection { get; set; }

    /// <summary>
    /// Target entity type
    /// </summary>
    public string TargetEntity { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key property name (if any)
    /// </summary>
    public string? ForeignKeyProperty { get; set; }
}
