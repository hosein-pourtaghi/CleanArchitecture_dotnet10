using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ====================================================================================
// CRUD Generator - Complete DDD CRUD Code Generator
// ====================================================================================

namespace CRUDGenerator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          DDD CRUD Generator for Clean Architecture              ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Simple argument parsing
        string? basePath = "F:\\Template Projects\\clean-architecture\\src\\";
        string? entityName = "Product";
        bool includeValidation = true;
        bool includeEvents = true;
        bool verbose = false;

        // Parse command line arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (i + 1 >= args.Length && args[i].ToLower() != "--verbose")
                break;

            switch (args[i].ToLower())
            {
                case "-b":
                case "--base":
                    basePath = args[++i];
                    break;
                case "-e":
                case "--entity":
                    entityName = args[++i];
                    break;
                case "-v":
                    includeValidation = bool.Parse(args[++i]);
                    break;
                case "-ev":
                    includeEvents = bool.Parse(args[++i]);
                    break;
                case "--verbose":
                    verbose = true;
                    break;
            }
        }

        // If not provided, prompt user
        if (string.IsNullOrEmpty(basePath))
        {
            Console.Write("Enter base path (e.g., F:\\Projects\\MyApp\\src): ");
            basePath = Console.ReadLine();
        }

        if (string.IsNullOrEmpty(entityName))
        {
            Console.Write("Enter entity name (e.g., Assessment): ");
            entityName = Console.ReadLine();
        }

        if (string.IsNullOrWhiteSpace(basePath) || string.IsNullOrWhiteSpace(entityName))
        {
            ShowUsage();
            return;
        }

        await RunGeneratorAsync(basePath, entityName, includeValidation, includeEvents, verbose);
    }


    static async Task RunGeneratorAsync(string basePath, string entityName, bool includeValidation, bool includeEvents, bool verbose)
    {
        try
        {
            // Find entity file
            var entityPath = Path.Combine(basePath, "Domain", "Aggregates", $"{entityName}s", $"{entityName}.cs");

            if (!File.Exists(entityPath))
            {
                // Try alternative path 
                entityPath = Path.Combine(basePath, "Domain", "Aggregates", $"{entityName}s", $"{entityName}Aggregate.cs");
            }

            if (!File.Exists(entityPath))
            {
                Console.WriteLine($"❌ Entity file not found: {entityPath}");
                return;
            }

            Console.WriteLine($"📄 Entity: {entityName}");
            Console.WriteLine($"📁 Base:   {basePath}");
            Console.WriteLine();

            // Parse entity
            var entity = EntityParser.ParseEntity(entityPath);
            if (entity == null)
            {
                Console.WriteLine("❌ Failed to parse entity");
                return;
            }

            var options = new GeneratorOptions
            {
                BasePath = basePath,
                ApplicationNamespace = "Application",
                DomainNamespace = "Domain",
                InfrastructureNamespace = "Infrastructure",
                IncludeValidation = includeValidation,
                IncludeDomainEvents = includeEvents
            };

            // Generate all files
            var files = CodeGenerator.GenerateAll(entity, options);
            var success = 0;

            foreach (var file in files)
            {
                try
                {
                    var dir = Path.GetDirectoryName(file.Path);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);

                    await File.WriteAllTextAsync(file.Path, file.Content);

                    Console.WriteLine($"✅ {file.RelativePath}");
                    success++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ {file.RelativePath}: {ex.Message}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"═══════════════════════════════════════════════════════════════════");
            Console.WriteLine($"   ✅ Generated {success} files");
            Console.WriteLine($"═══════════════════════════════════════════════════════════════════");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (verbose)
                Console.WriteLine(ex.StackTrace);
        }
    }
    static void ShowUsage()
    {
        Console.WriteLine();
        Console.WriteLine("❌ Error: Base path and entity name are required.");
        Console.WriteLine();
        Console.WriteLine("Usage: CRUDGenerator -b <base-path> -e <entity-name> [options]");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine(@"  CRUDGenerator -b ""F:\Projects\MyApp\src"" -e ""Assessment""");
    }
}

// ====================================================================================
// Models
// ====================================================================================

public class GeneratorOptions
{
    public string BasePath { get; set; } = "Domain";
    public string ApplicationNamespace { get; set; } = "Application";
    public string DomainNamespace { get; set; } = "Domain";
    public string InfrastructureNamespace { get; set; } = "Infrastructure";
    public bool IncludeValidation { get; set; } = true;
    public bool IncludeDomainEvents { get; set; } = true;
}

public class EntityInfo
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string BaseClass { get; set; } = "Entity";
    public List<EntityProperty> Properties { get; set; } = new();
    public List<NavigationProperty> NavigationProperties { get; set; } = new();
    public string IdType => Properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))?.Type ?? "Guid";
}

public class EntityProperty
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsCollection { get; set; }
    public bool HasSet { get; set; } = true;
    public bool IsReadOnly { get; set; }
}

public class NavigationProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsCollection { get; set; }
    public string? RelatedEntityName { get; set; }
    public string? ForeignKeyProperty { get; set; }
}

// ====================================================================================
// Entity Parser - Uses Roslyn to parse C# entity files
// ====================================================================================

public static class EntityParser
{
    public static readonly HashSet<string> SkipProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "RowVersion"
    };

    private static readonly HashSet<string> PrimitiveTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "int", "long", "short", "byte", "bool", "float", "double", "decimal",
        "Guid", "DateTime", "DateTimeOffset", "TimeSpan", "string", "char"
    };

    public static EntityInfo? ParseEntity(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var content = File.ReadAllText(filePath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetCompilationUnitRoot();

        var classDecl = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDecl == null)
            return null;

        var entity = new EntityInfo
        {
            Name = classDecl.Identifier.Text,
            Namespace = GetNamespace(root),
            BaseClass = GetBaseClass(classDecl)
        };

        // Parse properties
        foreach (var prop in classDecl.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (!IsValidProperty(prop))
                continue;

            var entityProp = new EntityProperty
            {
                Name = prop.Identifier.Text,
                Type = GetPropertyType(prop.Type),
                IsNullable = IsNullable(prop.Type),
                IsCollection = IsCollection(prop.Type),
                HasSet = HasSetAccessor(prop),
                IsReadOnly = !HasSetAccessor(prop)
            };

            entity.Properties.Add(entityProp);
        }

        // Parse navigation properties
        foreach (var prop in classDecl.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (!prop.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                continue;

            var typeStr = prop.Type.ToString();
            if (IsPrimitiveType(typeStr))
                continue;
            if (SkipProperties.Contains(prop.Identifier.Text))
                continue;

            var navProp = new NavigationProperty
            {
                Name = prop.Identifier.Text,
                Type = typeStr,
                IsCollection = IsCollection(prop.Type),
                RelatedEntityName = ExtractGenericType(typeStr)
            };

            // Find FK property
            var fkName = navProp.RelatedEntityName + "Id";
            if (entity.Properties.Any(p => p.Name.Equals(fkName, StringComparison.OrdinalIgnoreCase)))
            {
                navProp.ForeignKeyProperty = fkName;
            }

            entity.NavigationProperties.Add(navProp);
        }

        return entity;
    }

    static string GetNamespace(CompilationUnitSyntax root)
    {
        return root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
            .FirstOrDefault()?.Name.ToString() ?? "Domain";
    }

    static string GetBaseClass(ClassDeclarationSyntax classDecl)
    {
        var baseList = classDecl.BaseList;
        if (baseList == null || !baseList.Types.Any())
            return "Entity";
        return baseList.Types.First().Type.ToString();
    }

    static bool IsValidProperty(PropertyDeclarationSyntax prop)
    {
        if (!prop.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
            return false;
        if (prop.AccessorList == null)
            return false;
        if (!prop.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)))
            return false;
        return true;
    }

    static bool HasSetAccessor(PropertyDeclarationSyntax prop)
    {
        return prop.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false;
    }

    static string GetPropertyType(TypeSyntax type)
    {
        if (type is NullableTypeSyntax nullable)
            return nullable.ElementType.ToString() + "?";
        return type.ToString();
    }

    static bool IsNullable(TypeSyntax type)
    {
        if (type is NullableTypeSyntax)
            return true;
        return type.ToString().EndsWith("?");
    }

    static bool IsCollection(TypeSyntax type)
    {
        var typeStr = type.ToString();
        return typeStr.StartsWith("ICollection") ||
               typeStr.StartsWith("IEnumerable") ||
               typeStr.StartsWith("IList") ||
               typeStr.StartsWith("List") ||
               typeStr.StartsWith("HashSet");
    }

    static bool IsPrimitiveType(string typeName)
    {
        var clean = typeName.TrimEnd('?').TrimStart('<').TrimEnd('>');
        if (clean.Contains("<"))
        {
            var generic = ExtractGenericType(clean);
            return PrimitiveTypes.Contains(generic);
        }
        return PrimitiveTypes.Contains(clean) || clean == "string" || clean == "object";
    }

    static string ExtractGenericType(string typeStr)
    {
        var match = System.Text.RegularExpressions.Regex.Match(typeStr, @"<(\w+)>");
        return match.Success ? match.Groups[1].Value : typeStr;
    }
}
