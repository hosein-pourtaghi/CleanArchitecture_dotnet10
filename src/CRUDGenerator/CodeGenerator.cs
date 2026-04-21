namespace CRUDGenerator;


// ====================================================================================
// Code Generator - Generates separate files for each CRUD component
// ====================================================================================
public static class CodeGenerator
{
    private static readonly HashSet<string> SkipProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted", "RowVersion"
    };

    public static List<GeneratedFile> GenerateAll(EntityInfo entity, GeneratorOptions options)
    {
        var files = new List<GeneratedFile>();

        // Application layer files
        files.AddRange(GenerateApplicationFiles(entity, options));

        // Domain layer files
        files.AddRange(GenerateDomainFiles(entity, options));

        // Infrastructure layer files
        files.AddRange(GenerateInfrastructureFiles(entity, options));

        return files;
    }

    static List<GeneratedFile> GenerateApplicationFiles(EntityInfo entity, GeneratorOptions options)
    {
        var files = new List<GeneratedFile>();
        var basePath = options.BasePath;

        // DTOs
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", "Common", "DTOs", $"{entity.Name}s", $"{entity.Name}Dto.cs"),
            RelativePath = $"Application/Common/DTOs/{entity.Name}s/{entity.Name}Dto.cs",
            Content = GenerateDto(entity, options)
        });

        // Create Command
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", $"{entity.Name}s", "Create", $"Create{entity.Name}Command.cs"),
            RelativePath = $"Application/{entity.Name}s/Create/Create{entity.Name}Command.cs",
            Content = GenerateCreateCommand(entity, options)
        });

        // Create Handler
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", $"{entity.Name}s", "Create", $"Create{entity.Name}CommandHandler.cs"),
            RelativePath = $"Application/{entity.Name}s/Create/Create{entity.Name}CommandHandler.cs",
            Content = GenerateCreateHandler(entity, options)
        });

        // Update Command
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", $"{entity.Name}s", "Update", $"Update{entity.Name}Command.cs"),
            RelativePath = $"Application/{entity.Name}s/Update/Update{entity.Name}Command.cs",
            Content = GenerateUpdateCommand(entity, options)
        });

        // Update Handler
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", $"{entity.Name}s", "Update", $"Update{entity.Name}CommandHandler.cs"),
            RelativePath = $"Application/{entity.Name}s/Update/Update{entity.Name}CommandHandler.cs",
            Content = GenerateUpdateHandler(entity, options)
        });

        // Delete Command
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", $"{entity.Name}s", "Delete", $"Delete{entity.Name}Command.cs"),
            RelativePath = $"Application/{entity.Name}s/Delete/Delete{entity.Name}Command.cs",
            Content = GenerateDeleteCommand(entity, options)
        });

        // Delete Handler
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", $"{entity.Name}s", "Delete", $"Delete{entity.Name}CommandHandler.cs"),
            RelativePath = $"Application/{entity.Name}s/Delete/Delete{entity.Name}CommandHandler.cs",
            Content = GenerateDeleteHandler(entity, options)
        });

        // GetAll Query
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", $"{entity.Name}s", "GetAll", $"GetAll{entity.Name}Query.cs"),
            RelativePath = $"Application/{entity.Name}s/GetAll/GetAll{entity.Name}Query.cs",
            Content = GenerateGetAllQuery(entity, options)
        });

        // GetAll Handler
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", $"{entity.Name}s", "GetAll", $"GetAll{entity.Name}QueryHandler.cs"),
            RelativePath = $"Application/{entity.Name}s/GetAll/GetAll{entity.Name}QueryHandler.cs",
            Content = GenerateGetAllHandler(entity, options)
        });

        // GetById Query
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", $"{entity.Name}s", "GetById", $"GetById{entity.Name}Query.cs"),
            RelativePath = $"Application/{entity.Name}s/GetById/GetById{entity.Name}Query.cs",
            Content = GenerateGetByIdQuery(entity, options)
        });

        // GetById Handler
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", $"{entity.Name}s", "GetById", $"GetById{entity.Name}QueryHandler.cs"),
            RelativePath = $"Application/{entity.Name}s/GetById/GetById{entity.Name}QueryHandler.cs",
            Content = GenerateGetByIdHandler(entity, options)
        });

        // AutoMapper Profile
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Application", "Common", "Mappings", $"{entity.Name}Profile.cs"),
            RelativePath = $"Application/Common/Mappings/{entity.Name}Profile.cs",
            Content = GenerateMapperProfile(entity, options)
        });

        // Validators
        if (options.IncludeValidation)
        {
            files.Add(new GeneratedFile
            {
                Path = Path.Combine(basePath, "Application", "Common", "Validators", $"{entity.Name}s", $"{entity.Name}Validators.cs"),
                RelativePath = $"Application/Common/Validators/{entity.Name}s/{entity.Name}Validators.cs",
                Content = GenerateValidators(entity, options)
            });
        }

        return files;
    }

    static List<GeneratedFile> GenerateDomainFiles(EntityInfo entity, GeneratorOptions options)
    {
        var files = new List<GeneratedFile>();
        var basePath = options.BasePath;

        // Errors
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Domain", $"{entity.Name}s", $"{entity.Name}Errors.cs"),
            RelativePath = $"Domain/{entity.Name}s/{entity.Name}Errors.cs",
            Content = GenerateErrors(entity, options)
        });

        // Domain Events
        if (options.IncludeDomainEvents)
        {
            files.Add(new GeneratedFile
            {
                Path = Path.Combine(basePath, "Domain", $"{entity.Name}s", "Events", $"{entity.Name}Events.cs"),
                RelativePath = $"Domain/{entity.Name}s/Events/{entity.Name}Events.cs",
                Content = GenerateDomainEvents(entity, options)
            });
        }

        return files;
    }

    static List<GeneratedFile> GenerateInfrastructureFiles(EntityInfo entity, GeneratorOptions options)
    {
        var files = new List<GeneratedFile>();
        var basePath = options.BasePath;

        // EF Configuration
        files.Add(new GeneratedFile
        {
            Path = Path.Combine(basePath, "Infrastructure", "Persistence", "Configurations", $"{entity.Name}Configuration.cs"),
            RelativePath = $"Infrastructure/Persistence/Configurations/{entity.Name}Configuration.cs",
            Content = GenerateEfConfiguration(entity, options)
        });

        return files;
    }

    // ==================== Individual File Generators ====================

    static string GenerateDto(EntityInfo entity, GeneratorOptions options)
    {
        var props = string.Join("\n    ", entity.Properties
            .Where(p => !p.IsCollection)
            .Select(p => $"public {p.Type} {p.Name} {{ get; set; }}"));

        return $@"using System;

namespace {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;

/// <summary>
/// Response DTO for {entity.Name}
/// </summary>
public class {entity.Name}Dto
{{
{props}
}}
";
    }

    static string GenerateCreateCommand(EntityInfo entity, GeneratorOptions options)
    {
        var createProps = entity.Properties.Where(p => !SkipProperties.Contains(p.Name) && p.HasSet).ToList();
        var params_ = string.Join(",\n    ", createProps.Select(p => $"    {p.Type} {p.Name}"));

        return $@"using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;

namespace {options.ApplicationNamespace}.{entity.Name}s.Create;

/// <summary>
/// Command to create a new {entity.Name}
/// </summary>
public sealed record Create{entity.Name}Command(
{params_}
) : ICommand<{entity.IdType}>;
";
    }

    static string GenerateCreateHandler(EntityInfo entity, GeneratorOptions options)
    {
        var camel = ToCamel(entity.Name);

        return $@"using {options.ApplicationNamespace}.Common.Interfaces.Core;
using {options.ApplicationNamespace}.Common.Messaging;
using AutoMapper;
using {options.DomainNamespace}.{entity.Name}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace {options.ApplicationNamespace}.{entity.Name}s.Create;

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

        // {camel}.Raise(new {entity.Name}CreatedEvent({camel}.Id, DateTime.UtcNow));

        context.{entity.Name}s.Add({camel});
        await context.SaveChangesAsync(cancellationToken);

        return {camel}.Id;
    }}
}}
";
    }

    static string GenerateUpdateCommand(EntityInfo entity, GeneratorOptions options)
    {
        var updateProps = entity.Properties.Where(p => p.HasSet).ToList();
        var params_ = string.Join(",\n    ", updateProps.Select(p => $"    {p.Type} {p.Name}"));

        return $@"using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;

namespace {options.ApplicationNamespace}.{entity.Name}s.Update;

/// <summary>
/// Command to update an existing {entity.Name}
/// </summary>
public sealed record Update{entity.Name}Command(
{params_}
) : ICommand;
";
    }

    static string GenerateUpdateHandler(EntityInfo entity, GeneratorOptions options)
    {
        var camel = ToCamel(entity.Name);

        return $@"using {options.ApplicationNamespace}.Common.Interfaces.Core;
using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;
using AutoMapper;
using {options.DomainNamespace}.{entity.Name}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace {options.ApplicationNamespace}.{entity.Name}s.Update;

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

        // {camel}.Raise(new {entity.Name}UpdatedEvent({camel}.Id, DateTime.UtcNow));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }}
}}
";
    }

    static string GenerateDeleteCommand(EntityInfo entity, GeneratorOptions options)
    {
        return $@"using {options.ApplicationNamespace}.Common.Messaging;

namespace {options.ApplicationNamespace}.{entity.Name}s.Delete;

/// <summary>
/// Command to delete a {entity.Name}
/// </summary>
public sealed record Delete{entity.Name}Command({entity.IdType} Id) : ICommand;
";
    }

    static string GenerateDeleteHandler(EntityInfo entity, GeneratorOptions options)
    {
        var camel = ToCamel(entity.Name);

        return $@"using {options.ApplicationNamespace}.Common.Interfaces.Core;
using {options.ApplicationNamespace}.Common.Messaging;
using {options.DomainNamespace}.{entity.Name}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace {options.ApplicationNamespace}.{entity.Name}s.Delete;

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

        // {camel}.Raise(new {entity.Name}DeletedEvent({camel}.Id, DateTime.UtcNow));

        context.{entity.Name}s.Remove({camel});
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }}
}}
";
    }

    static string GenerateGetAllQuery(EntityInfo entity, GeneratorOptions options)
    {
        return $@"using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;

namespace {options.ApplicationNamespace}.{entity.Name}s.GetAll;

/// <summary>
/// Query to retrieve all {entity.Name}s with pagination
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

        return $@"using {options.ApplicationNamespace}.Common.Interfaces.Core;
using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace {options.ApplicationNamespace}.{entity.Name}s.GetAll;

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

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {{
            // Customize search: {camelPlural}Query = {camelPlural}Query.Where(x => x.Name.Contains(query.SearchTerm));
        }}

        var totalCount = await {camelPlural}Query.CountAsync(cancellationToken);

        {camelPlural}Query = ApplySorting({camelPlural}Query, query.SortBy, query.SortDescending);

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
            _ => descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        }};
    }}
}}
";
    }

    static string GenerateGetByIdQuery(EntityInfo entity, GeneratorOptions options)
    {
        return $@"using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;

namespace {options.ApplicationNamespace}.{entity.Name}s.GetById;

/// <summary>
/// Query to retrieve a {entity.Name} by ID
/// </summary>
public sealed record GetById{entity.Name}Query({entity.IdType} Id) : IQuery<{entity.Name}Dto>;
";
    }

    static string GenerateGetByIdHandler(EntityInfo entity, GeneratorOptions options)
    {
        var camel = ToCamel(entity.Name);

        return $@"using {options.ApplicationNamespace}.Common.Interfaces.Core;
using {options.ApplicationNamespace}.Common.Messaging;
using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;
using AutoMapper;
using {options.DomainNamespace}.{entity.Name}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace {options.ApplicationNamespace}.{entity.Name}s.GetById;

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

    static string GenerateMapperProfile(EntityInfo entity, GeneratorOptions options)
    {
        return $@"using {options.ApplicationNamespace}.Common.DTOs.{entity.Name}s;
using AutoMapper;
using {options.DomainNamespace}.{entity.Name}s;

namespace {options.ApplicationNamespace}.Common.Mappings;

/// <summary>
/// AutoMapper profile for {entity.Name}
/// </summary>
public class {entity.Name}Profile : Profile
{{
    public {entity.Name}Profile()
    {{
        CreateMap<{entity.Name}, {entity.Name}Dto>();
        CreateMap<Create{entity.Name}Dto, {entity.Name}>();
        CreateMap<Update{entity.Name}Dto, {entity.Name}>();
    }}
}}
";
    }

    static string GenerateValidators(EntityInfo entity, GeneratorOptions options)
    {
        var createProps = entity.Properties.Where(p => !SkipProperties.Contains(p.Name) && p.HasSet).ToList();
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

        var rules = validations.Count > 0 ? string.Join("\n", validations) : "        // Add rules here";

        return $@"using {options.ApplicationNamespace}.{entity.Name}s.Create;
using {options.ApplicationNamespace}.{entity.Name}s.Update;
using FluentValidation;

namespace {options.ApplicationNamespace}.Common.Validators.{entity.Name}s;

public class Create{entity.Name}CommandValidator : AbstractValidator<Create{entity.Name}Command>
{{
    public Create{entity.Name}CommandValidator()
    {{
{rules}
    }}
}}

public class Update{entity.Name}CommandValidator : AbstractValidator<Update{entity.Name}Command>
{{
    public Update{entity.Name}CommandValidator()
    {{
        RuleFor(x => x.Id).NotEmpty();
{rules}
    }}
}}
";
    }

    static string GenerateErrors(EntityInfo entity, GeneratorOptions options)
    {
        return $@"using SharedKernel;

namespace {options.DomainNamespace}.{entity.Name}s;

/// <summary>
/// Error definitions for {entity.Name}
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
}}
";
    }

    static string GenerateDomainEvents(EntityInfo entity, GeneratorOptions options)
    {
        return $@"using MediatR;

namespace {options.DomainNamespace}.{entity.Name}s.Events;

public record {entity.Name}CreatedEvent(
    {entity.IdType} {entity.Name}Id,
    DateTime OccurredAt
) : INotification;

public record {entity.Name}UpdatedEvent(
    {entity.IdType} {entity.Name}Id,
    DateTime OccurredAt
) : INotification;

public record {entity.Name}DeletedEvent(
    {entity.IdType} {entity.Name}Id,
    DateTime OccurredAt
) : INotification;
";
    }

    static string GenerateEfConfiguration(EntityInfo entity, GeneratorOptions options)
    {
        var propConfigs = new List<string>();

        foreach (var prop in entity.Properties.Where(p => !p.IsCollection && p.HasSet))
        {
            var configs = new List<string>();

            if (!IsNullableValueType(prop.Type) && prop.Type != "string" && !prop.IsNullable)
                configs.Add("IsRequired()");

            if (prop.Type == "string")
                configs.Add("HasMaxLength(500)");

            if (configs.Count > 0)
            {
                propConfigs.Add($@"
        builder.Property(x => x.{prop.Name})
            .{string.Join("\n            .", configs)};");
            }
        }

        var navConfigs = new List<string>();
        foreach (var nav in entity.NavigationProperties)
        {
            var deleteBehavior = nav.IsCollection ? "Cascade" : "Restrict";
            var fk = nav.ForeignKeyProperty ?? nav.Name + "Id";

            if (nav.IsCollection)
            {
                navConfigs.Add($@"
        builder.HasMany(x => x.{nav.Name})
            .WithOne()
            .HasForeignKey(x => x.{fk})
            .OnDelete(DeleteBehavior.{deleteBehavior});");
            }
            else
            {
                navConfigs.Add($@"
        builder.HasOne(x => x.{nav.Name})
            .WithMany()
            .HasForeignKey(x => x.{fk})
            .OnDelete(DeleteBehavior.{deleteBehavior});");
            }
        }

        var props = propConfigs.Count > 0 ? string.Join("", propConfigs) : "\n        // Configure properties here";
        var navs = navConfigs.Count > 0 ? string.Join("", navConfigs) : "\n        // Configure navigation here";

        return $@"using {options.DomainNamespace}.{entity.Name}s;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace {options.InfrastructureNamespace}.Persistence.Configurations;

public class {entity.Name}Configuration : IEntityTypeConfiguration<{entity.Name}>
{{
    public void Configure(EntityTypeBuilder<{entity.Name}> builder)
    {{
        builder.ToTable(""{entity.Name}s"");
        builder.HasKey(x => x.Id);
{props}
{navs}
        builder.HasIndex(x => x.Id);
    }}
}}
";
    }

    // Helper
    static string ToCamel(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLower(name[0]) + name.Substring(1);

    static bool IsNullableValueType(string type)
    {
        var clean = type.TrimEnd('?');
        return clean is "int" or "long" or "short" or "byte" or "bool" or "float" or "double" or "decimal"
            or "Guid" or "DateTime" or "DateTimeOffset" or "TimeSpan";
    }
}


public class GeneratedFile
{
    public string Path { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
