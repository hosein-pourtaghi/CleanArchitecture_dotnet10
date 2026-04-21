namespace CRUDGenerator.Models;

/// <summary>
/// Represents extracted information from a C# entity class
/// </summary>
public class EntityInfo
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public List<EntityProperty> Properties { get; set; } = new();
    public List<NavigationProperty> NavigationProperties { get; set; } = new();
    public string BaseClass { get; set; } = string.Empty;
    public bool HasId => Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
    public string IdType => Properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))?.Type ?? "Guid";
}

/// <summary>
/// Represents a property extracted from an entity
/// </summary>
public class EntityProperty
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsCollection { get; set; }
    public bool IsValueType { get; set; }
    public bool IsEnum { get; set; }
    public bool HasDefaultValue { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; }
    public bool IsReadOnly { get; set; }
    public bool HasSet { get; set; }
}

/// <summary>
/// Represents a navigation property (relationship) in an entity
/// </summary>
public class NavigationProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsCollection { get; set; }
    public string? ForeignKeyProperty { get; set; }
    public string? RelatedEntityName { get; set; }
    public DeleteBehavior? DeleteBehavior { get; set; }
}

public enum DeleteBehavior
{
    Cascade,
    Restrict,
    SetNull,
    NoAction
}
