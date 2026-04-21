using System;
//using System.CommandLine;
//using System.CommandLine.NamingConvention;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ====================================================================================
// CRUD Generator - Complete DDD CRUD Code Generator
// ====================================================================================

namespace CRUDGenerator;

class Program
{
    //static async Task<int> Main(string[] args)
    //{
    //    Console.WriteLine();
    //    Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
    //    Console.WriteLine("║          DDD CRUD Generator for Clean Architecture              ║");
    //    Console.WriteLine("║              .NET 8 | MediatR | EF Core | AutoMapper             ║");
    //    Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
    //    Console.WriteLine();

    //    // Define command line options
    //    var rootCommand = new RootCommand("DDD CRUD Generator - Generate complete CRUD boilerplate")
    //    {
    //        new Option<string>(
    //            aliases: new[] { "-i", "--input" },
    //            description: "Input directory containing entity files (e.g., Domain\\Aggregates)"
    //        ).Required(),

    //        new Option<string>(
    //            aliases: new[] { "-o", "--output" },
    //            description: "Output directory for generated files"
    //        ).Required(),

    //        new Option<string>(
    //            aliases: new[] { "-a", "--app-ns" },
    //            description: "Application namespace",
    //            getDefaultValue: () => "Application"
    //        ),

    //        new Option<string>(
    //            aliases: new[] { "-d", "--dom-ns" },
    //            description: "Domain namespace",
    //            getDefaultValue: () => "Domain"
    //        ),

    //        new Option<string>(
    //            aliases: new[] { "-inf", "--infra-ns" },
    //            description: "Infrastructure namespace",
    //            getDefaultValue: () => "Infrastructure"
    //        ),

    //        new Option<string>(
    //            aliases: new[] { "-e", "--entity" },
    //            description: "Specific entity name (generates all if not specified)"
    //        ),

    //        new Option<bool>(
    //            aliases: new[] { "-v", "--validation" },
    //            description: "Include FluentValidation",
    //            getDefaultValue: () => true
    //        ),

    //        new Option<bool>(
    //            aliases: new[] { "-ev", "--events" },
    //            description: "Include domain events",
    //            getDefaultValue: () => true
    //        ),

    //        new Option<bool>(
    //            aliases: new[] { "--verbose" },
    //            description: "Verbose output",
    //            getDefaultValue: () => false
    //        )
    //    };

    //    rootCommand.SetHandler(async context =>
    //    {
    //        var inputDir = context.ParseResult.GetValueForOption<string>("-i")!;
    //        var outputDir = context.ParseResult.GetValueForOption<string>("-o")!;
    //        var appNs = context.ParseResult.GetValueForOption<string>("-a")!;
    //        var domNs = context.ParseResult.GetValueForOption<string>("-d")!;
    //        var infraNs = context.ParseResult.GetValueForOption<string>("-inf")!;
    //        var entityName = context.ParseResult.GetValueForOption<string>("-e");
    //        var includeValidation = context.ParseResult.GetValueForOption<bool>("-v");
    //        var includeEvents = context.ParseResult.GetValueForOption<bool>("-ev");
    //        var verbose = context.ParseResult.GetValueForOption<bool>("--verbose");

    //        await RunGeneratorAsync(inputDir, outputDir, appNs, domNs, infraNs, entityName, includeValidation, includeEvents, verbose);
    //    });

    //    return await rootCommand.InvokeAsync(args);
    //}

    static async Task Main(string[] args)
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          DDD CRUD Generator for Clean Architecture              ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Simple argument parsing
        string? inputDir = null;
        string? outputDir = null;
        string appNs = "Application";
        string domNs = "Domain";
        string infraNs = "Infrastructure";
        string? entityName = null;
        bool includeValidation = true;
        bool includeEvents = true;
        bool verbose = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-i":
                case "--input":
                    inputDir = args[++i];
                    break;
                case "-o":
                case "--output":
                    outputDir = args[++i];
                    break;
                case "-a":
                case "--app-ns":
                    appNs = args[++i];
                    break;
                case "-d":
                case "--dom-ns":
                    domNs = args[++i];
                    break;
                case "-inf":
                case "--infra-ns":
                    infraNs = args[++i];
                    break;
                case "-e":
                case "--entity":
                    entityName = args[++i];
                    break;
                case "-v":
                case "--validation":
                    includeValidation = bool.Parse(args[++i]);
                    break;
                case "-ev":
                case "--events":
                    includeEvents = bool.Parse(args[++i]);
                    break;
                case "--verbose":
                    verbose = true;
                    break;
            }
        }

        if (string.IsNullOrEmpty(inputDir) || string.IsNullOrEmpty(outputDir))
        {
            Console.WriteLine("Usage: CRUDGenerator -i <input-dir> -o <output-dir> [options]");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine(@"  CRUDGenerator -i ""F:\Projects\MyApp\src\Domain"" -o ""F:\Projects\MyApp\Generated""");
            return;
        }

        await RunGeneratorAsync(inputDir, outputDir, appNs, domNs, infraNs, entityName, includeValidation, includeEvents, verbose);
    }

    static async Task RunGeneratorAsync(
        string inputDir,
        string outputDir,
        string appNs,
        string domNs,
        string infraNs,
        string? entityName,
        bool includeValidation,
        bool includeEvents,
        bool verbose)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(inputDir))
            {
                Console.WriteLine("❌ Error: Input directory is required.");
                ShowUsage();
                return;
            }

            if (!Directory.Exists(inputDir))
            {
                Console.WriteLine($"❌ Error: Input directory not found: {inputDir}");
                return;
            }

            // Create output directory
            Directory.CreateDirectory(outputDir);

            Console.WriteLine($"📁 Input:  {inputDir}");
            Console.WriteLine($"📁 Output: {outputDir}");
            Console.WriteLine();

            // Find entity files
            var csFiles = Directory.GetFiles(inputDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".Designer.cs") && !f.Contains("AssemblyInfo"))
                .ToList();

            if (csFiles.Count == 0)
            {
                Console.WriteLine($"❌ No C# files found in: {inputDir}");
                return;
            }

            // Filter by entity name if specified
            if (!string.IsNullOrWhiteSpace(entityName))
            {
                csFiles = csFiles.Where(f =>
                    Path.GetFileNameWithoutExtension(f).Equals(entityName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            Console.WriteLine($"🔍 Found {csFiles.Count} file(s) to process");
            Console.WriteLine();

            var options = new GeneratorOptions
            {
                ApplicationNamespace = appNs,
                DomainNamespace = domNs,
                InfrastructureNamespace = infraNs,
                IncludeValidation = includeValidation,
                IncludeDomainEvents = includeEvents
            };

            var successCount = 0;
            var errorCount = 0;

            foreach (var file in csFiles)
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (verbose)
                        Console.WriteLine($"📄 Processing: {fileName}");

                    var entity = EntityParser.ParseEntity(file);
                    if (entity == null)
                    {
                        Console.WriteLine($"  ⚠ Skipped: {fileName}");
                        continue;
                    }

                    if (verbose)
                    {
                        Console.WriteLine($"  ✓ Name: {entity.Name}");
                        Console.WriteLine($"  ✓ Properties: {entity.Properties.Count}");
                        Console.WriteLine($"  ✓ Navigation: {entity.NavigationProperties.Count}");
                    }

                    var code = CodeGenerator.Generate(entity, options);

                    var outputPath = Path.Combine(outputDir, $"{entity.Name}Crud.cs");
                    await File.WriteAllTextAsync(outputPath, code);

                    Console.WriteLine($"✅ Generated: {entity.Name}Crud.cs");
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {Path.GetFileName(file)} - {ex.Message}");
                    if (verbose)
                        Console.WriteLine(ex.StackTrace);
                    errorCount++;
                }
            }

            // Summary
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════════");
            Console.WriteLine("📊 GENERATION SUMMARY");
            Console.WriteLine("═══════════════════════════════════════════════════════════════════");
            Console.WriteLine($"   ✅ Success: {successCount}");
            Console.WriteLine($"   ❌ Errors:  {errorCount}");
            Console.WriteLine($"   📁 Output:  {outputDir}");
            Console.WriteLine("═══════════════════════════════════════════════════════════════════");

            if (successCount > 0)
            {
                Console.WriteLine();
                Console.WriteLine("💡 NEXT STEPS:");
                Console.WriteLine("   1. Review generated files in output directory");
                Console.WriteLine("   2. Add to your project structure:");
                Console.WriteLine("      • Application/{0}s/Create/", entityName ?? "Entity");
                Console.WriteLine("      • Application/{0}s/Update/", entityName ?? "Entity");
                Console.WriteLine("      • Application/{0}s/Delete/", entityName ?? "Entity");
                Console.WriteLine("      • Application/{0}s/GetAll/", entityName ?? "Entity");
                Console.WriteLine("      • Application/{0}s/GetById/", entityName ?? "Entity");
                Console.WriteLine("      • Application/Common/DTOs/{0}s/", entityName ?? "Entity");
                Console.WriteLine("      • Application/Common/Mappings/");
                Console.WriteLine("      • Application/Common/Validators/{0}s/", entityName ?? "Entity");
                Console.WriteLine("      • Domain/{0}s/", entityName ?? "Entity");
                Console.WriteLine("      • Infrastructure/Persistence/Configurations/");
                Console.WriteLine();
                Console.WriteLine("   3. Required NuGet packages:");
                Console.WriteLine("      dotnet add package MediatR");
                Console.WriteLine("      dotnet add package AutoMapper");
                Console.WriteLine("      dotnet add package FluentValidation");
                Console.WriteLine("      dotnet add package Microsoft.EntityFrameworkCore");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fatal error: {ex.Message}");
            if (verbose)
                Console.WriteLine(ex.StackTrace);
        }
    }

    static void ShowUsage()
    {
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  CRUDGenerator -i <input-dir> -o <output-dir> [options]");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine(@"  CRUDGenerator -i ""F:\Projects\MyApp\src\Domain\Aggregates"" -o ""F:\Projects\MyApp\Generated""");
    }
}

// ====================================================================================
// Models
// ====================================================================================

public class GeneratorOptions
{
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

// ====================================================================================
// Code Generator - Generates complete CRUD code
// ====================================================================================

public static class CodeGenerator
{
    public static string Generate(EntityInfo entity, GeneratorOptions options)
    {
        var code = $@"// ====================================================================================
// AUTO-GENERATED CRUD CODE - DO NOT MODIFY MANUALLY
// Entity: {entity.Name}
// Generated: {DateTime.Now} UTC
// ====================================================================================

";

        code += GenerateDtos(entity, options);
        code += GenerateCreateCommand(entity, options);
        code += GenerateCreateHandler(entity, options);
        code += GenerateUpdateCommand(entity, options);
        code += GenerateUpdateHandler(entity, options);
        code += GenerateDeleteCommand(entity, options);
        code += GenerateDeleteHandler(entity, options);
        code += GenerateGetAllQuery(entity, options);
        code += GenerateGetAllHandler(entity, options);
        code += GenerateGetByIdQuery(entity, options);
        code += GenerateGetByIdHandler(entity, options);
        code += GenerateMapperProfile(entity, options);
        code += GenerateErrors(entity, options);

        if (options.IncludeValidation)
            code += GenerateValidators(entity, options);

        if (options.IncludeDomainEvents)
            code += GenerateDomainEvents(entity, options);

        code += GenerateRepository(entity, options);
        code += GenerateEfConfiguration(entity, options);

        return code;
    }

    // ==================== DTOs ====================

    static string GenerateDtos(EntityInfo entity, GeneratorOptions options)
    {
        var createProps = entity.Properties.Where(p => !EntityParser.SkipProperties.Contains(p.Name) && p.HasSet).ToList();
        var updateProps = entity.Properties.Where(p => p.HasSet).ToList();

        var createParams = string.Join(",\n    ",
            createProps.Select(p => $"    {p.Type} {p.Name}"));

        var updateParams = string.Join(",\n    ",
            updateProps.Select(p => $"    {p.Type} {p.Name}"));

        var responseProps = string.Join("\n    ",
            entity.Properties.Where(p => !p.IsCollection).Select(p => $"public {p.Type} {p.Name} {{ get; set; }}"));

        return $@"
// ====================================================================================
// DTOs - Data Transfer Objects for {entity.Name}
// ====================================================================================

namespace {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;

/// <summary>
/// Response DTO for {entity.Name} - returned from queries
/// </summary>
public class {entity.Name}Dto
{{
{responseProps}
}}

/// <summary>
/// Create DTO for {entity.Name}
/// </summary>
public class Create{entity.Name}Dto
{{
{createParams}
}}

/// <summary>
/// Update DTO for {entity.Name}
/// </summary>
public class Update{entity.Name}Dto
{{
{updateParams}
}}

/// <summary>
/// List DTO for {entity.Name} - lightweight version for list views
/// </summary>
public class {entity.Name}ListDto
{{
    public {entity.IdType} Id {{ get; set; }}
}}

";
    }

    // ==================== Create Command ====================

    static string GenerateCreateCommand(EntityInfo entity, GeneratorOptions options)
    {
        var createProps = entity.Properties.Where(p => !EntityParser.SkipProperties.Contains(p.Name) && p.HasSet).ToList();
        var parameters = string.Join(",\n    ",
            createProps.Select(p => $"    {p.Type} {p.Name}"));

        return $@"
// ====================================================================================
// Create Command - MediatR command for creating {entity.Name}
// ====================================================================================

using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;

namespace {options.ApplicationNamespace}.{entity.Name}s.Create;

/// <summary>
/// Command to create a new {entity.Name} entity.
/// Inherits from ICommand&lt;Guid&gt; which returns Result&lt;Guid&gt;.
/// </summary>
public sealed record Create{entity.Name}Command(
{parameters}
) : ICommand<{entity.IdType}>;
";
    }

    static string GenerateCreateHandler(EntityInfo entity, GeneratorOptions options)
    {
        var camel = ToCamel(entity.Name);

        return $@"
// ====================================================================================
// Create Command Handler
// ====================================================================================

using {options.ApplicationNamespace}.Common.Interfaces.Core;
using {options.ApplicationNamespace}.Common.Messaging;
using AutoMapper;
using {options.DomainNamespace}.{entity.Name}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace {options.ApplicationNamespace}.{entity.Name}s.Create;

/// <summary>
/// Handles Create{entity.Name}Command requests.
/// </summary>
internal sealed class Create{entity.Name}CommandHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : ICommandHandler<Create{entity.Name}Command, {entity.IdType}>
{{
    public async Task<Result<{entity.IdType}>> Handle(
        Create{entity.Name}Command command,
        CancellationToken cancellationToken)
    {{
        var {camel} = mapper.Map<{entity.Name}>(command);

        // Publish domain event for audit/notification
        // {camel}.Raise(new {entity.Name}CreatedEvent({camel}.Id, DateTime.UtcNow));

        context.{entity.Name}s.Add({camel});
        await context.SaveChangesAsync(cancellationToken);

        return {camel}.Id;
    }}
}}

";
    }

    // ==================== Update Command ====================

    static string GenerateUpdateCommand(EntityInfo entity, GeneratorOptions options)
    {
        var updateProps = entity.Properties.Where(p => p.HasSet).ToList();
        var parameters = string.Join(",\n    ",
            updateProps.Select(p => $"    {p.Type} {p.Name}"));

        return $@"
// ====================================================================================
// Update Command - MediatR command for updating {entity.Name}
// ====================================================================================

using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;

namespace {options.ApplicationNamespace}.{entity.Name}s.Update;

/// <summary>
/// Command to update an existing {entity.Name} entity.
/// </summary>
public sealed record Update{entity.Name}Command(
{parameters}
) : ICommand;
";
    }

    static string GenerateUpdateHandler(EntityInfo entity, GeneratorOptions options)
    {
        var camel = ToCamel(entity.Name);

        return $@"
// ====================================================================================
// Update Command Handler
// ====================================================================================

using {options.ApplicationNamespace}.Common.Interfaces.Core;
using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;
using AutoMapper;
using {options.DomainNamespace}.{entity.Name}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace {options.ApplicationNamespace}.{entity.Name}s.Update;

/// <summary>
/// Handles Update{entity.Name}Command requests.
/// </summary>
internal sealed class Update{entity.Name}CommandHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : ICommandHandler<Update{entity.Name}Command>
{{
    public async Task<Result> Handle(
        Update{entity.Name}Command command,
        CancellationToken cancellationToken)
    {{
        var {camel} = await context.{entity.Name}s
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if ({camel} is null)
            return Result.Failure({entity.Name}Errors.NotFound(command.Id));

        mapper.Map(command, {camel});

        // Publish domain event
        // {camel}.Raise(new {entity.Name}UpdatedEvent({camel}.Id, DateTime.UtcNow));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }}
}}

";
    }

    // ==================== Delete Command ====================

    static string GenerateDeleteCommand(EntityInfo entity, GeneratorOptions options)
    {
        return $@"
// ====================================================================================
// Delete Command - MediatR command for deleting {entity.Name}
// ====================================================================================

using {options.ApplicationNamespace}.Common.Messaging;

namespace {options.ApplicationNamespace}.{entity.Name}s.Delete;

/// <summary>
/// Command to delete a {entity.Name} entity by ID.
/// </summary>
public sealed record Delete{entity.Name}Command({entity.IdType} Id) : ICommand;
";
    }

    static string GenerateDeleteHandler(EntityInfo entity, GeneratorOptions options)
    {
        var camel = ToCamel(entity.Name);

        return $@"
// ====================================================================================
// Delete Command Handler
// ====================================================================================

using {options.ApplicationNamespace}.Common.Interfaces.Core;
using {options.ApplicationNamespace}.Common.Messaging;
using {options.DomainNamespace}.{entity.Name}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace {options.ApplicationNamespace}.{entity.Name}s.Delete;

/// <summary>
/// Handles Delete{entity.Name}Command requests.
/// </summary>
internal sealed class Delete{entity.Name}CommandHandler(
    IApplicationDbContext context)
    : ICommandHandler<Delete{entity.Name}Command>
{{
    public async Task<Result> Handle(
        Delete{entity.Name}Command command,
        CancellationToken cancellationToken)
    {{
        var {camel} = await context.{entity.Name}s
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if ({camel} is null)
            return Result.Failure({entity.Name}Errors.NotFound(command.Id));

        // Publish domain event
        // {camel}.Raise(new {entity.Name}DeletedEvent({camel}.Id, DateTime.UtcNow));

        context.{entity.Name}s.Remove({camel});
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }}
}}

";
    }

    // ==================== GetAll Query ====================

    static string GenerateGetAllQuery(EntityInfo entity, GeneratorOptions options)
    {
        return $@"
// ====================================================================================
// GetAll Query - MediatR query for retrieving all {entity.Name}s
// ====================================================================================

using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;

namespace {options.ApplicationNamespace}.{entity.Name}s.GetAll;

/// <summary>
/// Query to retrieve all {entity.Name} entities with pagination.
/// </summary>
public sealed record GetAll{entity.Name}Query(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false
) : IQuery<PaginatedResult<{entity.Name}Dto>>;
";
    }

    static string GenerateGetAllHandler(EntityInfo entity, GeneratorOptions options)
    {
        var camelPlural = ToCamel(entity.Name) + "s";

        return $@"
// ====================================================================================
// GetAll Query Handler
// ====================================================================================

using {options.ApplicationNamespace}.Common.Interfaces.Core;
using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace {options.ApplicationNamespace}.{entity.Name}s.GetAll;

/// <summary>
/// Handles GetAll{entity.Name}Query requests.
/// </summary>
internal sealed class GetAll{entity.Name}QueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetAll{entity.Name}Query, PaginatedResult<{entity.Name}Dto>>
{{
    public async Task<Result<PaginatedResult<{entity.Name}Dto>>> Handle(
        GetAll{entity.Name}Query query,
        CancellationToken cancellationToken)
    {{
        var {camelPlural}Query = context.{entity.Name}s.AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {{
            // Customize search based on entity properties
            // {camelPlural}Query = {camelPlural}Query.Where(x => x.Name.Contains(query.SearchTerm));
        }}

        var totalCount = await {camelPlural}Query.CountAsync(cancellationToken);

        // Apply sorting
        {camelPlural}Query = ApplySorting({camelPlural}Query, query.SortBy, query.SortDescending);

        // Apply pagination
        var skip = (query.Page - 1) * query.PageSize;
        var {camelPlural} = await {camelPlural}Query
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var {camelPlural}Dtos = mapper.Map<List<{entity.Name}Dto>>({camelPlural});

        return new PaginatedResult<{entity.Name}Dto>(
            items: {camelPlural}Dtos,
            totalCount: totalCount,
            page: query.Page,
            pageSize: query.PageSize
        );
    }}

    private static IQueryable<{entity.Name}> ApplySorting(
        IQueryable<{entity.Name}> query,
        string? sortBy,
        bool descending)
    {{
        if (string.IsNullOrWhiteSpace(sortBy))
            return descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id);

        return sortBy.ToLowerInvariant() switch
        {{
            // Add sortable properties here
            // ""name"" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            _ => descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        }};
    }}
}}

";
    }

    // ==================== GetById Query ====================

    static string GenerateGetByIdQuery(EntityInfo entity, GeneratorOptions options)
    {
        return $@"
// ====================================================================================
// GetById Query - MediatR query for retrieving a single {entity.Name}
// ====================================================================================

using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;

namespace {options.ApplicationNamespace}.{entity.Name}s.GetById;

/// <summary>
/// Query to retrieve a {entity.Name} by its unique identifier.
/// </summary>
public sealed record GetById{entity.Name}Query({entity.IdType} Id) : IQuery<{entity.Name}Dto>;
";
    }

    static string GenerateGetByIdHandler(EntityInfo entity, GeneratorOptions options)
    {
        var camel = ToCamel(entity.Name);

        return $@"
// ====================================================================================
// GetById Query Handler
// ====================================================================================

using {options.ApplicationNamespace}.Common.Interfaces.Core;
using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;
using AutoMapper;
using {options.DomainNamespace}.{entity.Name}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace {options.ApplicationNamespace}.{entity.Name}s.GetById;

/// <summary>
/// Handles GetById{entity.Name}Query requests.
/// </summary>
internal sealed class GetById{entity.Name}QueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetById{entity.Name}Query, {entity.Name}Dto>
{{
    public async Task<Result<{entity.Name}Dto>> Handle(
        GetById{entity.Name}Query query,
        CancellationToken cancellationToken)
    {{
        var {camel} = await context.{entity.Name}s
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

        if ({camel} is null)
            return Result.Failure<{entity.Name}Dto>({entity.Name}Errors.NotFound(query.Id));

        return mapper.Map<{entity.Name}Dto>({camel});
    }}
}}

";
    }

    // ==================== AutoMapper Profile ====================

    static string GenerateMapperProfile(EntityInfo entity, GeneratorOptions options)
    {
        return $@"
// ====================================================================================
// AutoMapper Profile - Mapping configuration for {entity.Name}
// ====================================================================================

using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;
using AutoMapper;
using {options.DomainNamespace}.{entity.Name}s;

namespace {options.ApplicationNamespace}.Common.Mappings;

/// <summary>
/// AutoMapper profile for {entity.Name} entity and DTOs.
/// </summary>
public class {entity.Name}Profile : Profile
{{
    public {entity.Name}Profile()
    {{
        // Entity to DTO
        CreateMap<{entity.Name}, {entity.Name}Dto>();
        CreateMap<{entity.Name}, {entity.Name}ListDto>();

        // Create DTO to Entity
        CreateMap<Create{entity.Name}Dto, {entity.Name}>();

        // Update DTO to Entity (for partial updates)
        CreateMap<Update{entity.Name}Dto, {entity.Name}>();
    }}
}}

";
    }

    // ==================== Errors ====================

    static string GenerateErrors(EntityInfo entity, GeneratorOptions options)
    {
        return $@"
// ====================================================================================
// Domain Errors - Error definitions for {entity.Name}
// ====================================================================================

using SharedKernel;

namespace {options.DomainNamespace}.{entity.Name}s;

/// <summary>
/// Static class containing error definitions for {entity.Name} operations.
/// </summary>
public static class {entity.Name}Errors
{{
    public static Error NotFound({entity.IdType} id) =>
        Error.NotFound(
            code: ""{entity.Name}.NotFound"",
            message: $""The {{typeof({entity.Name}).Name}} with ID '{{id}}' was not found."");

    public static Error Duplicate({entity.IdType} id) =>
        Error.Conflict(
            code: ""{entity.Name}.Duplicate"",
            message: $""A {{typeof({entity.Name}).Name}} with ID '{{id}}' already exists."");

    public static Error Validation(string message) =>
        Error.Validation(
            code: ""{entity.Name}.Validation"",
            message: message);
}}

";
    }

    // ==================== FluentValidation ====================

    static string GenerateValidators(EntityInfo entity, GeneratorOptions options)
    {
        var createProps = entity.Properties.Where(p => !EntityParser.SkipProperties.Contains(p.Name) && p.HasSet).ToList();
        var validations = new List<string>();

        foreach (var prop in createProps)
        {
            if (prop.Type == "string")
            {
                validations.Add($@"
        RuleFor(x => x.{prop.Name})
            .NotEmpty()
            .WithMessage(""{prop.Name} is required."");");
            }
            else if (!IsNullableValueType(prop.Type))
            {
                validations.Add($@"
        RuleFor(x => x.{prop.Name})
            .NotNull()
            .WithMessage(""{prop.Name} is required."");");
            }
        }

        var validationRules = validations.Count > 0 ? string.Join("\n", validations) : "        // Add validation rules here";

        return $@"
// ====================================================================================
// FluentValidation - Validation rules for {entity.Name} commands
// ====================================================================================

using {options.ApplicationNamespace}.{entity.Name}s.Create;
using {options.ApplicationNamespace}.{entity.Name}s.Update;
using FluentValidation;

namespace {options.ApplicationNamespace}.Common.Validators.{entity.Name}s;

/// <summary>
/// Validator for Create{entity.Name}Command.
/// </summary>
public class Create{entity.Name}CommandValidator : AbstractValidator<Create{entity.Name}Command>
{{
    public Create{entity.Name}CommandValidator()
    {{
{validationRules}
    }}
}}

/// <summary>
/// Validator for Update{entity.Name}Command.
/// </summary>
public class Update{entity.Name}CommandValidator : AbstractValidator<Update{entity.Name}Command>
{{
    public Update{entity.Name}CommandValidator()
    {{
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(""Id is required for update"");
{validationRules}
    }}
}}

";
    }

    // ==================== Domain Events ====================

    static string GenerateDomainEvents(EntityInfo entity, GeneratorOptions options)
    {
        return $@"
// ====================================================================================
// Domain Events - Events raised by {entity.Name} aggregate
// ====================================================================================

using MediatR;

namespace {options.DomainNamespace}.{entity.Name}s.Events;

/// <summary>
/// Event raised when a new {entity.Name} is created.
/// </summary>
public record {entity.Name}CreatedEvent(
    {entity.IdType} {entity.Name}Id,
    DateTime OccurredAt
) : INotification;

/// <summary>
/// Event raised when a {entity.Name} is updated.
/// </summary>
public record {entity.Name}UpdatedEvent(
    {entity.IdType} {entity.Name}Id,
    DateTime OccurredAt
) : INotification;

/// <summary>
/// Event raised when a {entity.Name} is deleted.
/// </summary>
public record {entity.Name}DeletedEvent(
    {entity.IdType} {entity.Name}Id,
    DateTime OccurredAt
) : INotification;

";
    }

    // ==================== Repository ====================

    static string GenerateRepository(EntityInfo entity, GeneratorOptions options)
    {
        return $@"
// ====================================================================================
// Repository Interface - Data access abstraction for {entity.Name}
// ====================================================================================

using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;
using {options.ApplicationNamespace}.Common.Interfaces.Core;
using SharedKernel;

namespace {options.ApplicationNamespace}.Common.Interfaces.Repositories.{entity.Name}s;

/// <summary>
/// Repository interface for {entity.Name} entity.
/// </summary>
public interface I{entity.Name}Repository : IBaseRepository<{entity.Name}>
{{
    Task<{entity.Name}?> GetByIdWithIncludesAsync({entity.IdType} id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync({entity.IdType} id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<{entity.Name}Dto>> GetPaginatedAsync(
        int page, int pageSize, string? searchTerm = null,
        string? sortBy = null, bool sortDescending = false,
        CancellationToken cancellationToken = default);
}}

/*
// Implementation in Infrastructure project:
public class {entity.Name}Repository : BaseRepository<{entity.Name}>, I{entity.Name}Repository
{{
    public {entity.Name}Repository(ApplicationDbContext context, IMapper mapper) : base(context, mapper) {{ }}

    public async Task<{entity.Name}?> GetByIdWithIncludesAsync({entity.IdType} id, CancellationToken cancellationToken)
    {{
        return await _context.{entity.Name}s
            .Include(x => x.RelatedEntity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }}

    public async Task<bool> ExistsAsync({entity.IdType} id, CancellationToken cancellationToken)
    {{
        return await _context.{entity.Name}s.AnyAsync(x => x.Id == id, cancellationToken);
    }}

    public async Task<PaginatedResult<{entity.Name}Dto>> GetPaginatedAsync(
        int page, int pageSize, string? searchTerm, string? sortBy, bool sortDescending, CancellationToken cancellationToken)
    {{
        return await GetAllAsync<{entity.Name}Dto>(new PaginatedRequest
        {{
            Page = page, PageSize = pageSize, SearchTerm = searchTerm,
            SortBy = sortBy, SortDescending = sortDescending
        }}, cancellationToken);
    }}
}}
*/

";
    }

    // ==================== EF Core Configuration ====================

    static string GenerateEfConfiguration(EntityInfo entity, GeneratorOptions options)
    {
        var propertyConfigs = new List<string>();

        foreach (var prop in entity.Properties.Where(p => !p.IsCollection && p.HasSet))
        {
            var configs = new List<string>();

            if (!IsNullableValueType(prop.Type) && prop.Type != "string" && !prop.IsNullable)
                configs.Add("IsRequired()");

            if (prop.Type == "string")
                configs.Add("HasMaxLength(500)");

            if (configs.Count > 0)
            {
                propertyConfigs.Add($@"
        builder.Property(x => x.{prop.Name})
            .{string.Join("\n            .", configs)};");
            }
        }

        var navConfigs = new List<string>();
        foreach (var nav in entity.NavigationProperties)
        {
            if (nav.IsCollection)
            {
                navConfigs.Add($@"
        builder.HasMany(x => x.{nav.Name})
            .WithOne()
            .HasForeignKey(x => x.{nav.ForeignKeyProperty ?? nav.Name + "Id"})
            .OnDelete(DeleteBehavior.Cascade);");
            }
            else
            {
                navConfigs.Add($@"
        builder.HasOne(x => x.{nav.Name})
            .WithMany()
            .HasForeignKey(x => x.{nav.ForeignKeyProperty ?? nav.Name + "Id"})
            .OnDelete(DeleteBehavior.Restrict);");
            }
        }

        var propSection = propertyConfigs.Count > 0 ? string.Join("", propertyConfigs) : "\n        // Configure properties here";
        var navSection = navConfigs.Count > 0 ? string.Join("", navConfigs) : "\n        // Configure navigation properties here";

        return $@"
// ====================================================================================
// Entity Framework Core Configuration - Database mapping for {entity.Name}
// ====================================================================================

using {options.DomainNamespace}.{entity.Name}s;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace {options.InfrastructureNamespace}.Persistence.Configurations;

/// <summary>
/// EF Core configuration for {entity.Name} entity.
/// </summary>
public class {entity.Name}Configuration : IEntityTypeConfiguration<{entity.Name}>
{{
    public void Configure(EntityTypeBuilder<{entity.Name}> builder)
    {{
        builder.ToTable(""{entity.Name}s"");
        builder.HasKey(x => x.Id);
{propSection}
{navSection}
        builder.HasIndex(x => x.Id);
    }}
}}

";
    }

    // ==================== Helper Methods ====================

    static string ToCamel(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length == 1)
            return name.ToLower();
        return char.ToLower(name[0]) + name.Substring(1);
    }

    static bool IsNullableValueType(string type)
    {
        var clean = type.TrimEnd('?');
        return clean is "int" or "long" or "short" or "byte" or "bool" or "float" or "double" or "decimal" or
               "Guid" or "DateTime" or "DateTimeOffset" or "TimeSpan";
    }
}
