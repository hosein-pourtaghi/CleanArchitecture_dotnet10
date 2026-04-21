// =====================================================================
// CRUDGenerator - DDD CRUD Code Generator for Clean Architecture
// =====================================================================
// This console application reads C# entity/aggregate classes and generates
// complete CRUD boilerplate code including:
// - DTOs (Create, Update, Response)
// - MediatR Commands and Queries
// - Command/Query Handlers
// - AutoMapper Profiles
// - FluentValidation Rules
// - EF Core Configurations
// - Repository Interfaces
// - Domain Events
// =====================================================================

using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using CRUDGenerator.Models;
using CRUDGenerator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRUDGenerator;

public class Program
{
    private static ILogger<Program>? _logger;
    private static IServiceProvider? _serviceProvider;

    public static async Task<int> Main(string[] args)
    {
        // Configure dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            _logger.LogInformation("CRUDGenerator v1.0.0 - Starting...");

            // Parse command line arguments
            var rootCommand = CreateRootCommand();
            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            _logger?.LogCritical(ex, "Fatal error occurred during execution");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"FATAL ERROR: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
        finally
        {
            _logger?.LogInformation("CRUDGenerator - Execution completed");
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddConsole();
        });

        // Add services
        services.AddSingleton<IEntityParser, EntityParser>();
        services.AddSingleton<ICodeGenerator, CodeGenerator>();
        services.AddSingleton<IFileService, FileService>();
    }

    private static RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand
        {
            Name = "CRUDGenerator",
            Description = "DDD CRUD Code Generator for Clean Architecture with MediatR, AutoMapper, and EF Core"
        };

        // Input directory option
        var inputOption = new Option<string>(
            aliases: new[] { "-i", "--input", "--input-dir" },
            description: "Input directory containing entity files",
            getDefaultValue: () => Directory.GetCurrentDirectory())
        {
            IsRequired = false
        };

        // Output directory option
        var outputOption = new Option<string>(
            aliases: new[] { "-o", "--output", "--output-dir" },
            description: "Output directory for generated files",
            getDefaultValue: () => Path.Combine(Directory.GetCurrentDirectory(), "Generated"))
        {
            IsRequired = false
        };

        // Entity name option (optional - if not provided, processes all entities)
        var entityOption = new Option<string>(
            aliases: new[] { "-e", "--entity", "--entity-name" },
            description: "Specific entity name to generate (optional - processes all if not specified)")
        {
            IsRequired = false
        };

        // Namespace option
        var namespaceOption = new Option<string>(
            aliases: new[] { "-n", "--namespace", "--ns" },
            description: "Base namespace for generated code",
            getDefaultValue: () => "YourApp")
        {
            IsRequired = false
        };

        // Single file output option
        var singleFileOption = new Option<bool>(
            aliases: new[] { "-s", "--single-file" },
            description: "Generate all code in a single file per entity (default: true)")
        {
            IsRequired = false
        };

        // Overwrite option
        var overwriteOption = new Option<bool>(
            aliases: new[] { "-w", "--overwrite" },
            description: "Overwrite existing files")
        {
            IsRequired = false
        };

        // Include validation option
        var validationOption = new Option<bool>(
            aliases: new[] { "-v", "--validation" },
            description: "Include FluentValidation rules")
        {
            IsRequired = false
        };

        // Include domain events option
        var domainEventsOption = new Option<bool>(
            aliases: new[] { "-d", "--domain-events" },
            description: "Include domain event handlers")
        {
            IsRequired = false
        };

        // Include repository option
        var repositoryOption = new Option<bool>(
            aliases: new[] { "-r", "--repository" },
            description: "Include repository interface")
        {
            IsRequired = false
        };

        rootCommand.AddOption(inputOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(entityOption);
        rootCommand.AddOption(namespaceOption);
        rootCommand.AddOption(singleFileOption);
        rootCommand.AddOption(overwriteOption);
        rootCommand.AddOption(validationOption);
        rootCommand.AddOption(domainEventsOption);
        rootCommand.AddOption(repositoryOption);

        rootCommand.SetHandler(async (context) =>
        {
            var inputDir = context.ParseResult.GetValueForOption(inputOption)!;
            var outputDir = context.ParseResult.GetValueForOption(outputOption)!;
            var entityName = context.ParseResult.GetValueForOption(entityOption);
            var baseNamespace = context.ParseResult.GetValueForOption(namespaceOption)!;
            var singleFile = context.ParseResult.GetValueForOption(singleFileOption);
            var overwrite = context.ParseResult.GetValueForOption(overwriteOption);
            var includeValidation = context.ParseResult.GetValueForOption(validationOption);
            var includeDomainEvents = context.ParseResult.GetValueForOption(domainEventsOption);
            var includeRepository = context.ParseResult.GetValueForOption(repositoryOption);

            var options = new GenerationOptions
            {
                InputDirectory = inputDir,
                OutputDirectory = outputDir,
                EntityName = entityName,
                BaseNamespace = baseNamespace,
                GenerateSingleFile = singleFile,
                OverwriteExisting = overwrite,
                IncludeValidation = includeValidation,
                IncludeDomainEvents = includeDomainEvents,
                IncludeRepository = includeRepository
            };

            await GenerateCrudAsync(options);
        });

        return rootCommand;
    }

    private static async Task GenerateCrudAsync(GenerationOptions options)
    {
        _logger?.LogInformation("========================================");
        _logger?.LogInformation("CRUDGenerator - Starting Code Generation");
        _logger?.LogInformation("========================================");
        _logger?.LogInformation("Input Directory: {InputDir}", options.InputDirectory);
        _logger?.LogInformation("Output Directory: {OutputDir}", options.OutputDirectory);
        _logger?.LogInformation("Base Namespace: {Namespace}", options.BaseNamespace);
        _logger?.LogInformation("Single File Mode: {SingleFile}", options.GenerateSingleFile);
        _logger?.LogInformation("Include Validation: {Validation}", options.IncludeValidation);
        _logger?.LogInformation("Include Domain Events: {DomainEvents}", options.IncludeDomainEvents);
        _logger?.LogInformation("Include Repository: {Repository}", options.IncludeRepository);

        var entityParser = _serviceProvider!.GetRequiredService<IEntityParser>();
        var codeGenerator = _serviceProvider!.GetRequiredService<ICodeGenerator>();
        var fileService = _serviceProvider!.GetRequiredService<IFileService>();

        // Validate input directory
        if (!Directory.Exists(options.InputDirectory))
        {
            _logger?.LogError("Input directory does not exist: {InputDir}", options.InputDirectory);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: Input directory does not exist: {options.InputDirectory}");
            Console.ResetColor();
            return;
        }

        // Create output directory if it doesn't exist
        Directory.CreateDirectory(options.OutputDirectory);

        // Find entity files
        var entityFiles = fileService.FindEntityFiles(options.InputDirectory, options.EntityName);

        if (entityFiles.Count == 0)
        {
            _logger?.LogWarning("No entity files found in {InputDir}", options.InputDirectory);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"WARNING: No entity files found in {options.InputDirectory}");
            Console.ResetColor();
            return;
        }

        _logger?.LogInformation("Found {Count} entity file(s) to process", entityFiles.Count);

        var successCount = 0;
        var errorCount = 0;

        foreach (var entityFile in entityFiles)
        {
            try
            {
                _logger?.LogInformation("Processing entity: {EntityFile}", entityFile);

                // Parse entity
                var entityInfo = await entityParser.ParseEntityFileAsync(entityFile, options.BaseNamespace);

                if (entityInfo == null)
                {
                    _logger?.LogWarning("Failed to parse entity file: {EntityFile}", entityFile);
                    errorCount++;
                    continue;
                }

                _logger?.LogInformation("Parsed entity '{Name}' with {PropCount} properties",
                    entityInfo.Name, entityInfo.Properties.Count);

                // Generate code
                var generatedFiles = await codeGenerator.GenerateCrudAsync(entityInfo, options);

                // Save files
                foreach (var (filePath, content) in generatedFiles)
                {
                    await fileService.SaveFileAsync(filePath, content, options.OverwriteExisting);
                }

                successCount++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Generated CRUD for '{entityInfo.Name}' ({generatedFiles.Count} file(s))");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing entity file: {EntityFile}", entityFile);
                errorCount++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error processing {entityFile}: {ex.Message}");
                Console.ResetColor();
            }
        }

        _logger?.LogInformation("========================================");
        _logger?.LogInformation("Code Generation Complete");
        _logger?.LogInformation("Successful: {Success}, Errors: {Errors}", successCount, errorCount);
        _logger?.LogInformation("Output Directory: {OutputDir}", options.OutputDirectory);
        _logger?.LogInformation("========================================");

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
