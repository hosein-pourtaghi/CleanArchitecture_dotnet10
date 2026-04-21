// =====================================================================
// EntityParser - Parses C# entity files using Roslyn
// =====================================================================

using System.Text.RegularExpressions;
using CRUDGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace CRUDGenerator.Services;

/// <summary>
/// Interface for entity parsing service
/// </summary>
public interface IEntityParser
{
    /// <summary>
    /// Parse an entity file and extract metadata
    /// </summary>
    Task<EntityInfo?> ParseEntityFileAsync(string filePath, string baseNamespace);
}

/// <summary>
/// Parses C# entity files using Roslyn
/// </summary>
public class EntityParser : IEntityParser
{
    private readonly ILogger<EntityParser> _logger;

    public EntityParser(ILogger<EntityParser> logger)
    {
        _logger = logger;
    }

    public async Task<EntityInfo?> ParseEntityFileAsync(string filePath, string baseNamespace)
    {
        try
        {
            _logger.LogDebug("Parsing entity file: {FilePath}", filePath);

            var content = await File.ReadAllTextAsync(filePath);
            var syntaxTree = CSharpSyntaxTree.ParseText(content);
            var root = syntaxTree.GetCompilationUnitRoot();

            // Find the class declaration
            var classDeclaration = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            if (classDeclaration == null)
            {
                _logger.LogWarning("No class declaration found in {FilePath}", filePath);
                return null;
            }

            var entityInfo = new EntityInfo
            {
                Name = classDeclaration.Identifier.Text,
                FilePath = filePath,
                Namespace = ExtractNamespace(root) ?? baseNamespace
            };

            // Extract base class
            entityInfo.BaseClass = ExtractBaseClass(classDeclaration);

            // Extract properties
            entityInfo.Properties = ExtractProperties(classDeclaration);

            // Extract navigation properties
            entityInfo.NavigationProperties = ExtractNavigationProperties(classDeclaration);

            _logger.LogDebug("Parsed entity '{Name}' with {PropCount} properties and {NavCount} navigation properties",
                entityInfo.Name, entityInfo.Properties.Count, entityInfo.NavigationProperties.Count);

            return entityInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing entity file: {FilePath}", filePath);
            return null;
        }
    }

    private static string? ExtractNamespace(CompilationUnitSyntax root)
    {
        var namespaceDeclaration = root.DescendantNodes()
            .OfType<NamespaceDeclarationSyntax>()
            .FirstOrDefault();

        return namespaceDeclaration?.Name.ToString();
    }

    private static string? ExtractBaseClass(ClassDeclarationSyntax classDeclaration)
    {
        var baseList = classDeclaration.BaseList;
        if (baseList == null || baseList.Types.Count == 0)
            return null;

        return baseList.Types[0].Type.ToString();
    }

    private static List<EntityProperty> ExtractProperties(ClassDeclarationSyntax classDeclaration)
    {
        var properties = new List<EntityProperty>();

        foreach (var member in classDeclaration.Members)
        {
            if (member is not PropertyDeclarationSyntax propertyDeclaration)
                continue;

            // Skip if not public
            if (!propertyDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                continue;

            // Skip if no getter/setter
            if (propertyDeclaration.AccessorList == null)
                continue;

            var hasGet = propertyDeclaration.AccessorList.Accessors
                .Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
            var hasSet = propertyDeclaration.AccessorList.Accessors
                .Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));

            if (!hasGet && !hasSet)
                continue;

            var prop = new EntityProperty
            {
                Name = propertyDeclaration.Identifier.Text,
                Type = propertyDeclaration.Type.ToString(),
                IsNullable = IsNullable(propertyDeclaration.Type),
                IsValueType = IsValueType(propertyDeclaration.Type),
                IsCollection = IsCollection(propertyDeclaration.Type.ToString()),
                IsRequired = IsRequired(propertyDeclaration),
                IsEnum = IsEnumType(propertyDeclaration.Type.ToString())
            };

            // Extract max length from attributes
            prop.MaxLength = ExtractMaxLength(propertyDeclaration);

            properties.Add(prop);
        }

        return properties;
    }

    private static bool IsNullable(TypeSyntax type)
    {
        // Handle nullable value types (int?, Guid?)
        if (type is NullableTypeSyntax)
            return true;

        // Handle nullable reference types (string?)
        var typeText = type.ToString().Trim();
        return typeText.EndsWith("?");
    }

    private static bool IsValueType(TypeSyntax type)
    {
        var typeText = type.ToString().Trim();

        // Remove nullable annotation for checking
        if (typeText.EndsWith("?"))
            typeText = typeText[..^1];

        var valueTypes = new HashSet<string>
        {
            "int", "long", "short", "byte", "sbyte",
            "float", "double", "decimal",
            "bool", "char", "DateTime", "DateTimeOffset",
            "TimeSpan", "Guid"
        };

        return valueTypes.Contains(typeText);
    }

    private static bool IsCollection(string typeName)
    {
        var collectionPatterns = new[]
        {
            @"^ICollection<",
            @"^IEnumerable<",
            @"^IList<",
            @"^List<",
            @"^IReadOnlyList<",
            @"^IReadOnlyCollection<",
            @"^HashSet<",
            @"^ICollection<"
        };

        return collectionPatterns.Any(pattern =>
            Regex.IsMatch(typeName, pattern, RegexOptions.IgnoreCase));
    }

    private static bool IsRequired(PropertyDeclarationSyntax property)
    {
        // Check for [Required] attribute
        var attributes = property.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(a => a.Name.ToString());

        if (attributes.Contains("Required", StringComparer.OrdinalIgnoreCase))
            return true;

        // Check if it's a value type without nullable annotation
        var typeText = property.Type.ToString().Trim();
        if (!typeText.EndsWith("?"))
        {
            var valueTypes = new HashSet<string>
            {
                "int", "long", "short", "byte", "sbyte",
                "float", "double", "decimal", "bool", "char"
            };
            if (valueTypes.Contains(typeText))
                return true;
        }

        return false;
    }

    private static bool IsEnumType(string typeName)
    {
        // Simple check - in real scenario, would need to resolve the type
        return typeName.Contains("Enum", StringComparison.OrdinalIgnoreCase) ||
               char.IsUpper(typeName.FirstOrDefault());
    }

    private static int? ExtractMaxLength(PropertyDeclarationSyntax property)
    {
        foreach (var attributeList in property.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attrName = attribute.Name.ToString();
                if (attrName.Contains("MaxLength", StringComparison.OrdinalIgnoreCase))
                {
                    // Try to extract the length from arguments
                    if (attribute.ArgumentList?.Arguments.Count > 0)
                    {
                        var arg = attribute.ArgumentList.Arguments[0];
                        if (int.TryParse(arg.Expression.GetText().ToString(), out var length))
                            return length;
                    }
                    return 256; // Default max length
                }
            }
        }
        return null;
    }

    private static List<NavigationProperty> ExtractNavigationProperties(ClassDeclarationSyntax classDeclaration)
    {
        var navigationProperties = new List<NavigationProperty>();

        // Look for navigation property patterns
        foreach (var member in classDeclaration.Members)
        {
            if (member is not PropertyDeclarationSyntax property)
                continue;

            if (!property.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                continue;

            var typeName = property.Type.ToString();

            // Check if it looks like a navigation property
            // (has virtual keyword or is a collection/reference type)
            if (IsNavigationPropertyType(typeName))
            {
                var navProp = new NavigationProperty
                {
                    Name = property.Identifier.Text,
                    Type = typeName,
                    IsCollection = IsCollection(typeName),
                    TargetEntity = ExtractTargetEntity(typeName)
                };

                navigationProperties.Add(navProp);
            }
        }

        return navigationProperties;
    }

    private static bool IsNavigationPropertyType(string typeName)
    {
        // Check for common navigation property patterns
        if (IsCollection(typeName))
            return true;

        // Check if it's a class type (not a primitive)
        var primitives = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "string", "int", "long", "short", "byte", "sbyte",
            "float", "double", "decimal", "bool", "char",
            "DateTime", "DateTimeOffset", "TimeSpan", "Guid",
            "object"
        };

        // Extract base type if it's nullable
        var baseType = typeName.TrimEnd('?');

        // Check if it's a generic type (likely a navigation property)
        if (baseType.Contains('<'))
            return true;

        return !primitives.Contains(baseType);
    }

    private static string ExtractTargetEntity(string typeName)
    {
        // Extract entity name from collection or reference type
        var match = Regex.Match(typeName, @"(\w+)(?:<(\w+)>)?");
        if (match.Success)
        {
            // For ICollection<Entity>, return Entity
            if (match.Groups[2].Success)
                return match.Groups[2].Value;

            // For Entity, return Entity
            return match.Groups[1].Value;
        }

        return typeName;
    }
}
