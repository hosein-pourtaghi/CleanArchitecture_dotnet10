// =====================================================================
// CodeGenerator - Generates CRUD code from entity information
// =====================================================================

using System.Text;
using CRUDGenerator.Models;
using Microsoft.Extensions.Logging;

namespace CRUDGenerator.Services;

/// <summary>
/// Interface for code generation service
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Generate CRUD code for an entity
    /// </summary>
    Task<List<(string FilePath, string Content)>> GenerateCrudAsync(EntityInfo entity, GenerationOptions options);
}

/// <summary>
/// Generates CRUD code from entity information
/// </summary>
public class CodeGenerator : ICodeGenerator
{
    private readonly ILogger<CodeGenerator> _logger;

    public CodeGenerator(ILogger<CodeGenerator> logger)
    {
        _logger = logger;
    }

    public async Task<List<(string FilePath, string Content)>> GenerateCrudAsync(
        EntityInfo entity,
        GenerationOptions options)
    {
        var files = new List<(string FilePath, string Content)>();

        _logger.LogInformation("Generating CRUD code for entity: {EntityName}", entity.Name);

        if (options.GenerateSingleFile)
        {
            // Generate single file with all CRUD code
            var (filePath, content) = GenerateSingleFile(entity, options);
            files.Add((filePath, content));
        }
        else
        {
            // Generate separate files
            files.AddRange(GenerateSeparateFiles(entity, options));
        }

        _logger.LogInformation("Generated {Count} file(s) for entity: {EntityName}",
            files.Count, entity.Name);

        return await Task.FromResult(files);
    }

    private (string FilePath, string Content) GenerateSingleFile(EntityInfo entity, GenerationOptions options)
    {
        var sb = new StringBuilder();

        // Add file header
        sb.AppendLine("/*");
        sb.AppendLine(" * ============================================================================");
        sb.AppendLine($" * AUTO-GENERATED CRUD CODE FOR: {entity.Name}");
        sb.AppendLine($" * Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($" * Generator: CRUDGenerator");
        sb.AppendLine(" * ============================================================================");
        sb.AppendLine(" * ");
        sb.AppendLine(" * This file contains complete CRUD implementation including:");
        sb.AppendLine(" * - DTOs (Create, Update, Response)");
        sb.AppendLine(" * - MediatR Commands and Queries");
        sb.AppendLine(" * - Command/Query Handlers");
        sb.AppendLine(" * - AutoMapper Profile");
        if (options.IncludeValidation)
            sb.AppendLine(" * - FluentValidation Rules");
        if (options.IncludeDomainEvents)
            sb.AppendLine(" * - Domain Events");
        if (options.IncludeRepository)
            sb.AppendLine(" * - Repository Interface");
        sb.AppendLine(" * ");
        sb.AppendLine(" * DO NOT MODIFY THIS FILE - Regenerate from source entity");
        sb.AppendLine(" * ============================================================================");
        sb.AppendLine(" */");
        sb.AppendLine();

        // Add using statements
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();
        sb.AppendLine("using Application.Common.DTOs;");
        sb.AppendLine("using Application.Common.Interfaces.Core;");
        sb.AppendLine("using Application.Common.Messaging;");
        sb.AppendLine("using AutoMapper;");
        sb.AppendLine("using Domain.Aggregates;");
        sb.AppendLine("using FluentValidation;");
        sb.AppendLine("using MediatR;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using SharedKernel;");
        sb.AppendLine();

        // Generate DTOs
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.Common.DTOs;");
        sb.AppendLine();
        sb.Append(GenerateCreateDto(entity));
        sb.Append(GenerateUpdateDto(entity));
        sb.Append(GenerateResponseDto(entity));
        sb.AppendLine();

        // Generate MediatR Commands
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.{entity.Name}s.Create;");
        sb.AppendLine();
        sb.Append(GenerateCreateCommand(entity));
        sb.AppendLine();
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.{entity.Name}s.Update;");
        sb.AppendLine();
        sb.Append(GenerateUpdateCommand(entity));
        sb.AppendLine();
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.{entity.Name}s.Delete;");
        sb.AppendLine();
        sb.Append(GenerateDeleteCommand(entity));
        sb.AppendLine();

        // Generate MediatR Queries
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.{entity.Name}s.GetAll;");
        sb.AppendLine();
        sb.Append(GenerateGetAllQuery(entity));
        sb.AppendLine();
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.{entity.Name}s.GetById;");
        sb.AppendLine();
        sb.Append(GenerateGetByIdQuery(entity));
        sb.AppendLine();

        // Generate Handlers
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.{entity.Name}s.Create;");
        sb.AppendLine();
        sb.Append(GenerateCreateHandler(entity, options));
        sb.AppendLine();
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.{entity.Name}s.Update;");
        sb.AppendLine();
        sb.Append(GenerateUpdateHandler(entity, options));
        sb.AppendLine();
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.{entity.Name}s.Delete;");
        sb.AppendLine();
        sb.Append(GenerateDeleteHandler(entity, options));
        sb.AppendLine();
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.{entity.Name}s.GetAll;");
        sb.AppendLine();
        sb.Append(GenerateGetAllHandler(entity, options));
        sb.AppendLine();
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.{entity.Name}s.GetById;");
        sb.AppendLine();
        sb.Append(GenerateGetByIdHandler(entity, options));
        sb.AppendLine();

        // Generate AutoMapper Profile
        sb.AppendLine($"namespace {options.BaseNamespace}.Application.Common.Mappings;");
        sb.AppendLine();
        sb.Append(GenerateAutoMapperProfile(entity));
        sb.AppendLine();

        // Generate FluentValidation
        if (options.IncludeValidation)
        {
            sb.AppendLine($"namespace {options.BaseNamespace}.Application.{entity.Name}s.Validators;");
            sb.AppendLine();
            sb.Append(GenerateValidators(entity));
            sb.AppendLine();
        }

        // Generate Domain Events
        if (options.IncludeDomainEvents)
        {
            sb.AppendLine($"namespace {options.BaseNamespace}.Domain.Aggregates.{entity.Name}s.Events;");
            sb.AppendLine();
            sb.Append(GenerateDomainEvents(entity));
            sb.AppendLine();
        }

        // Generate Repository Interface
        if (options.IncludeRepository)
        {
            sb.AppendLine($"namespace {options.BaseNamespace}.Application.Common.Interfaces;");
            sb.AppendLine();
            sb.Append(GenerateRepositoryInterface(entity));
            sb.AppendLine();
        }

        // Generate Errors
        sb.AppendLine($"namespace {options.BaseNamespace}.Domain.Aggregates.{entity.Name}s;");
        sb.AppendLine();
        sb.Append(GenerateErrors(entity));
        sb.AppendLine();

        var fileName = $"{entity.Name}Crud.cs";
        var filePath = Path.Combine(options.OutputDirectory, fileName);

        return (filePath, sb.ToString());
    }

    private List<(string FilePath, string Content)> GenerateSeparateFiles(EntityInfo entity, GenerationOptions options)
    {
        var files = new List<(string FilePath, string Content)>();

        // DTOs
        var dtoDir = Path.Combine(options.OutputDirectory, "DTOs");
        files.Add((Path.Combine(dtoDir, $"{entity.Name}CreateDto.cs"), GenerateCreateDto(entity)));
        files.Add((Path.Combine(dtoDir, $"{entity.Name}UpdateDto.cs"), GenerateUpdateDto(entity)));
        files.Add((Path.Combine(dtoDir, $"{entity.Name}ResponseDto.cs"), GenerateResponseDto(entity)));

        // Commands
        var createDir = Path.Combine(options.OutputDirectory, "Commands", entity.Name);
        files.Add((Path.Combine(createDir, $"Create{entity.Name}Command.cs"), GenerateCreateCommand(entity)));
        files.Add((Path.Combine(createDir, $"Update{entity.Name}Command.cs"), GenerateUpdateCommand(entity)));
        files.Add((Path.Combine(createDir, $"Delete{entity.Name}Command.cs"), GenerateDeleteCommand(entity)));

        // Queries
        var queryDir = Path.Combine(options.OutputDirectory, "Queries", entity.Name);
        files.Add((Path.Combine(queryDir, $"GetAll{entity.Name}Query.cs"), GenerateGetAllQuery(entity)));
        files.Add((Path.Combine(queryDir, $"GetById{entity.Name}Query.cs"), GenerateGetByIdQuery(entity)));

        // Handlers
        files.Add((Path.Combine(createDir, $"Create{entity.Name}CommandHandler.cs"),
            GenerateCreateHandler(entity, options)));
        files.Add((Path.Combine(createDir, $"Update{entity.Name}CommandHandler.cs"),
            GenerateUpdateHandler(entity, options)));
        files.Add((Path.Combine(createDir, $"Delete{entity.Name}CommandHandler.cs"),
            GenerateDeleteHandler(entity, options)));
        files.Add((Path.Combine(queryDir, $"GetAll{entity.Name}QueryHandler.cs"),
            GenerateGetAllHandler(entity, options)));
        files.Add((Path.Combine(queryDir, $"GetById{entity.Name}QueryHandler.cs"),
            GenerateGetByIdHandler(entity, options)));

        // AutoMapper Profile
        var mappingDir = Path.Combine(options.OutputDirectory, "Mappings");
        files.Add((Path.Combine(mappingDir, $"{entity.Name}Profile.cs"), GenerateAutoMapperProfile(entity)));

        // Validators
        if (options.IncludeValidation)
        {
            var validatorDir = Path.Combine(options.OutputDirectory, "Validators", entity.Name);
            files.Add((Path.Combine(validatorDir, $"Create{entity.Name}Validator.cs"),
                GenerateCreateValidator(entity)));
            files.Add((Path.Combine(validatorDir, $"Update{entity.Name}Validator.cs"),
                GenerateUpdateValidator(entity)));
        }

        // Domain Events
        if (options.IncludeDomainEvents)
        {
            var eventsDir = Path.Combine(options.OutputDirectory, "Events", entity.Name);
            files.Add((Path.Combine(eventsDir, $"{entity.Name}CreatedEvent.cs"),
                GenerateCreatedEvent(entity)));
            files.Add((Path.Combine(eventsDir, $"{entity.Name}UpdatedEvent.cs"),
                GenerateUpdatedEvent(entity)));
            files.Add((Path.Combine(eventsDir, $"{entity.Name}DeletedEvent.cs"),
                GenerateDeletedEvent(entity)));
        }

        // Repository
        if (options.IncludeRepository)
        {
            var repoDir = Path.Combine(options.OutputDirectory, "Repositories");
            files.Add((Path.Combine(repoDir, $"I{entity.Name}Repository.cs"),
                GenerateRepositoryInterface(entity)));
        }

        // Errors
        files.Add((Path.Combine(options.OutputDirectory, $"{entity.Name}Errors.cs"),
            GenerateErrors(entity)));

        return files;
    }

    #region DTOs

    private string GenerateCreateDto(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// DTO for creating a new {entity.Name}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed record {entity.Name}CreateDto(");

        var props = entity.CreateProperties.ToList();
        for (int i = 0; i < props.Count; i++)
        {
            var prop = props[i];
            var separator = i < props.Count - 1 ? "," : ");";
            sb.AppendLine($"    {prop.GetTypeWithAnnotation()} {prop.Name}{separator}");
        }

        return sb.ToString();
    }

    private string GenerateUpdateDto(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// DTO for updating an existing {entity.Name}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed record {entity.Name}UpdateDto(");

        var props = entity.UpdateProperties.ToList();
        for (int i = 0; i < props.Count; i++)
        {
            var prop = props[i];
            var separator = i < props.Count - 1 ? "," : ");";
            sb.AppendLine($"    {prop.GetTypeWithAnnotation()} {prop.Name}{separator}");
        }

        return sb.ToString();
    }

    private string GenerateResponseDto(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Response DTO for {entity.Name}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed class {entity.Name}ResponseDto");
        sb.AppendLine("{");

        foreach (var prop in entity.ResponseProperties)
        {
            sb.AppendLine($"    public {prop.GetDtoType()} {prop.Name} {{ get; set; }}");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    #endregion

    #region Commands

    private string GenerateCreateCommand(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Command to create a new {entity.Name}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed record Create{entity.Name}Command(");

        var props = entity.CreateProperties.ToList();
        for (int i = 0; i < props.Count; i++)
        {
            var prop = props[i];
            var separator = i < props.Count - 1 ? "," : ");";
            sb.AppendLine($"    {prop.GetTypeWithAnnotation()} {ToCamelCase(prop.Name)}{separator}");
        }

        sb.AppendLine($"    : ICommand<Guid>;");
        return sb.ToString();
    }

    private string GenerateUpdateCommand(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Command to update an existing {entity.Name}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed record Update{entity.Name}Command(");

        var props = entity.UpdateProperties.ToList();
        for (int i = 0; i < props.Count; i++)
        {
            var prop = props[i];
            var separator = i < props.Count - 1 ? "," : ");";
            sb.AppendLine($"    {prop.GetTypeWithAnnotation()} {ToCamelCase(prop.Name)}{separator}");
        }

        sb.AppendLine($"    : ICommand;");
        return sb.ToString();
    }

    private string GenerateDeleteCommand(EntityInfo entity)
    {
        return $@"/// <summary>
/// Command to delete a {entity.Name} by ID
/// </summary>
public sealed record Delete{entity.Name}Command(Guid Id) : ICommand;
";
    }

    #endregion

    #region Queries

    private string GenerateGetAllQuery(EntityInfo entity)
    {
        return $@"/// <summary>
/// Query to retrieve all {entity.Name} entities with pagination
/// </summary>
public sealed record GetAll{entity.Name}Query(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false    ) : IQuery<PaginatedResult<{entity.Name}ResponseDto>>;
";
    }

    private string GenerateGetByIdQuery(EntityInfo entity)
    {
        return $@"/// <summary>
/// Query to retrieve a {entity.Name} by its unique identifier
/// </summary>
public sealed record GetById{entity.Name}Query(Guid Id) : IQuery<{entity.Name}ResponseDto>;
";
    }

    #endregion

    #region Handlers

    private string GenerateCreateHandler(EntityInfo entity, GenerationOptions options)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Handler for Create{entity.Name}Command");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"internal sealed class Create{entity.Name}CommandHandler(");
        sb.AppendLine("    IApplicationDbContext context,");
        sb.AppendLine("    IMapper mapper)");
        sb.AppendLine($"    : ICommandHandler<Create{entity.Name}Command, Guid>");
        sb.AppendLine("{");
        sb.AppendLine("    public async Task<Result<Guid>> Handle(");
        sb.AppendLine($"        Create{entity.Name}Command command,");
        sb.AppendLine("        CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine("        // Map command to entity");
        sb.AppendLine($"        var {ToCamelCase(entity.Name)} = mapper.Map<{entity.Name}>(command);");
        sb.AppendLine();

        if (options.IncludeDomainEvents)
        {
            sb.AppendLine("        // Raise domain event for audit logging and async operations");
            sb.AppendLine($"        {ToCamelCase(entity.Name)}.Raise(new {entity.Name}CreatedEvent(");
            sb.AppendLine($"            {ToCamelCase(entity.Name)}.Id,");
            foreach (var prop in entity.CreateProperties.Take(5))
            {
                sb.AppendLine($"            {ToCamelCase(prop.Name)}: {ToCamelCase(entity.Name)}.{prop.Name},");
            }
            sb.AppendLine("        ));");
            sb.AppendLine();
        }

        sb.AppendLine("        // Persist to database");
        sb.AppendLine($"        context.{entity.Name}s.Add({ToCamelCase(entity.Name)});");
        sb.AppendLine("        await context.SaveChangesAsync(cancellationToken);");
        sb.AppendLine();
        sb.AppendLine($"        return {ToCamelCase(entity.Name)}.Id;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateUpdateHandler(EntityInfo entity, GenerationOptions options)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Handler for Update{entity.Name}Command");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"internal sealed class Update{entity.Name}CommandHandler(");
        sb.AppendLine("    IApplicationDbContext context,");
        sb.AppendLine("    IMapper mapper)");
        sb.AppendLine($"    : ICommandHandler<Update{entity.Name}Command>");
        sb.AppendLine("{");
        sb.AppendLine("    public async Task<Result> Handle(");
        sb.AppendLine($"        Update{entity.Name}Command command,");
        sb.AppendLine("        CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var {ToCamelCase(entity.Name)} = await context.{entity.Name}s");
        sb.AppendLine($"            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine($"        if ({ToCamelCase(entity.Name)} is null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return Result.Failure({entity.Name}Errors.NotFound(command.Id));");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // Update entity with new values using AutoMapper");
        sb.AppendLine($"        mapper.Map(command, {ToCamelCase(entity.Name)});");
        sb.AppendLine();

        if (options.IncludeDomainEvents)
        {
            sb.AppendLine("        // Raise domain event for audit logging");
            sb.AppendLine($"        {ToCamelCase(entity.Name)}.Raise(new {entity.Name}UpdatedEvent(");
            sb.AppendLine($"            {ToCamelCase(entity.Name)}.Id,");
            foreach (var prop in entity.UpdateProperties.Take(5))
            {
                sb.AppendLine($"            {ToCamelCase(prop.Name)}: {ToCamelCase(entity.Name)}.{prop.Name},");
            }
            sb.AppendLine("        ));");
            sb.AppendLine();
        }

        sb.AppendLine("        await context.SaveChangesAsync(cancellationToken);");
        sb.AppendLine("        return Result.Success();");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateDeleteHandler(EntityInfo entity, GenerationOptions options)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Handler for Delete{entity.Name}Command");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"internal sealed class Delete{entity.Name}CommandHandler(");
        sb.AppendLine("    IApplicationDbContext context)");
        sb.AppendLine($"    : ICommandHandler<Delete{entity.Name}Command>");
        sb.AppendLine("{");
        sb.AppendLine("    public async Task<Result> Handle(");
        sb.AppendLine($"        Delete{entity.Name}Command command,");
        sb.AppendLine("        CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var {ToCamelCase(entity.Name)} = await context.{entity.Name}s");
        sb.AppendLine($"            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine($"        if ({ToCamelCase(entity.Name)} is null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return Result.Failure({entity.Name}Errors.NotFound(command.Id));");
        sb.AppendLine("        }");
        sb.AppendLine();

        if (options.IncludeDomainEvents)
        {
            sb.AppendLine("        // Capture data before deletion for event publishing");
            sb.AppendLine($"        var deletedEvent = new {entity.Name}DeletedEvent(");
            sb.AppendLine($"            {ToCamelCase(entity.Name)}.Id");
            foreach (var prop in entity.Properties.Where(p => !p.IsId && !p.IsCollection).Take(5))
            {
                sb.AppendLine($"            , {ToCamelCase(prop.Name)}: {ToCamelCase(entity.Name)}.{prop.Name}");
            }
            sb.AppendLine("        );");
            sb.AppendLine();
            sb.AppendLine($"        {ToCamelCase(entity.Name)}.Raise(deletedEvent);");
            sb.AppendLine();
        }

        sb.AppendLine($"        context.{entity.Name}s.Remove({ToCamelCase(entity.Name)});");
        sb.AppendLine("        await context.SaveChangesAsync(cancellationToken);");
        sb.AppendLine("        return Result.Success();");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateGetAllHandler(EntityInfo entity, GenerationOptions options)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Handler for GetAll{entity.Name}Query with pagination and filtering");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"internal sealed class GetAll{entity.Name}QueryHandler(");
        sb.AppendLine("    IApplicationDbContext context,");
        sb.AppendLine("    IMapper mapper)");
        sb.AppendLine($"    : IQueryHandler<GetAll{entity.Name}Query, PaginatedResult<{entity.Name}ResponseDto>>");
        sb.AppendLine("{");
        sb.AppendLine("    public async Task<Result<PaginatedResult<{entity.Name}ResponseDto>>> Handle(");
        sb.AppendLine($"        GetAll{entity.Name}Query query,");
        sb.AppendLine("        CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var queryable = context.{entity.Name}s.AsNoTracking();");
        sb.AppendLine();
        sb.AppendLine("        // Apply search filter if provided");
        sb.AppendLine("        if (!string.IsNullOrWhiteSpace(query.SearchTerm))");
        sb.AppendLine("        {");
        sb.AppendLine("            // Implement search logic based on entity properties");
        sb.AppendLine("            // queryable = queryable.Where(x => x.Name.Contains(query.SearchTerm));");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // Get total count for pagination");
        sb.AppendLine("        var totalCount = await queryable.CountAsync(cancellationToken);");
        sb.AppendLine();
        sb.AppendLine("        // Apply sorting");
        sb.AppendLine("        queryable = ApplySorting(queryable, query.SortBy, query.SortDescending);");
        sb.AppendLine();
        sb.AppendLine("        // Apply pagination");
        sb.AppendLine("        var skip = (query.Page - 1) * query.PageSize;");
        sb.AppendLine("        var items = await queryable");
        sb.AppendLine("            .Skip(skip)");
        sb.AppendLine("            .Take(query.PageSize)");
        sb.AppendLine("            .ToListAsync(cancellationToken);");
        sb.AppendLine();
        sb.AppendLine("        // Map to response DTOs");
        sb.AppendLine($"        var dtos = mapper.Map<List<{entity.Name}ResponseDto}>(items);");
        sb.AppendLine();
        sb.AppendLine($"        var result = new PaginatedResult<{entity.Name}ResponseDto>(");
        sb.AppendLine("            dtos,");
        sb.AppendLine("            totalCount,");
        sb.AppendLine("            query.Page,");
        sb.AppendLine("            query.PageSize");
        sb.AppendLine("        );");
        sb.AppendLine();
        sb.AppendLine("        return result;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static IQueryable<{entity.Name}> ApplySorting(");
        sb.AppendLine("        IQueryable<{entity.Name}> query,");
        sb.AppendLine("        string? sortBy,");
        sb.AppendLine("        bool descending)");
        sb.AppendLine("    {");
        sb.AppendLine("        return sortBy?.ToLowerInvariant() switch");
        sb.AppendLine("        {");

        foreach (var prop in entity.Properties.Where(p => !p.IsCollection))
        {
            sb.AppendLine($"            \"{prop.Name.ToLowerInvariant()}\" => descending");
            sb.AppendLine($"                ? query.OrderByDescending(x => x.{prop.Name})");
            sb.AppendLine($"                : query.OrderBy(x => x.{prop.Name}),");
        }

        sb.AppendLine("            _ => query.OrderByDescending(x => x.Id)");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateGetByIdHandler(EntityInfo entity, GenerationOptions options)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Handler for GetById{entity.Name}Query");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"internal sealed class GetById{entity.Name}QueryHandler(");
        sb.AppendLine("    IApplicationDbContext context,");
        sb.AppendLine("    IMapper mapper)");
        sb.AppendLine($"    : IQueryHandler<GetById{entity.Name}Query, {entity.Name}ResponseDto>");
        sb.AppendLine("{");
        sb.AppendLine("    public async Task<Result<{entity.Name}ResponseDto>> Handle(");
        sb.AppendLine($"        GetById{entity.Name}Query query,");
        sb.AppendLine("        CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var {ToCamelCase(entity.Name)} = await context.{entity.Name}s");
        sb.AppendLine("            .AsNoTracking()");
        sb.AppendLine($"            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);");
        sb.AppendLine();
        sb.AppendLine($"        if ({ToCamelCase(entity.Name)} is null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return Result.Failure<{entity.Name}ResponseDto>({entity.Name}Errors.NotFound(query.Id));");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        var dto = mapper.Map<{entity.Name}ResponseDto>({ToCamelCase(entity.Name)});");
        sb.AppendLine("        return dto;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    #endregion

    #region AutoMapper Profile

    private string GenerateAutoMapperProfile(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// AutoMapper profile for {entity.Name} entity and DTOs");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {entity.Name}Profile : Profile");
        sb.AppendLine("{");
        sb.AppendLine($"    public {entity.Name}Profile()");
        sb.AppendLine("    {");
        sb.AppendLine($"        // Entity to Response DTO mapping");
        sb.AppendLine($"        CreateMap<{entity.Name}, {entity.Name}ResponseDto>();");
        sb.AppendLine();
        sb.AppendLine($"        // Create Command to Entity mapping");
        sb.AppendLine($"        CreateMap<Create{entity.Name}Command, {entity.Name}>()");
        sb.AppendLine($"            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()));");
        sb.AppendLine();
        sb.AppendLine($"        // Update Command to Entity mapping (for updates)");
        sb.AppendLine($"        CreateMap<Update{entity.Name}Command, {entity.Name}>(MemberList.Source)");
        sb.AppendLine($"            .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember, context) =>");
        sb.AppendLine("                srcMember != null));");
        sb.AppendLine();
        sb.AppendLine($"        // Create DTO to Entity mapping");
        sb.AppendLine($"        CreateMap<{entity.Name}CreateDto, {entity.Name}>()");
        sb.AppendLine($"            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()));");
        sb.AppendLine();
        sb.AppendLine($"        // Update DTO to Entity mapping");
        sb.AppendLine($"        CreateMap<{entity.Name}UpdateDto, {entity.Name}>(MemberList.Source);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    #endregion

    #region FluentValidation

    private string GenerateValidators(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.Append(GenerateCreateValidator(entity));
        sb.AppendLine();
        sb.Append(GenerateUpdateValidator(entity));
        return sb.ToString();
    }

    private string GenerateCreateValidator(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// FluentValidation validator for Create{entity.Name}Command");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class Create{entity.Name}CommandValidator : AbstractValidator<Create{entity.Name}Command>");
        sb.AppendLine("{");
        sb.AppendLine($"    public Create{entity.Name}CommandValidator()");
        sb.AppendLine("    {");
        sb.AppendLine($"        RuleFor(x => x)");
        sb.AppendLine("            .NotNull()");
        sb.AppendLine("            .WithMessage(\"Command cannot be null\");");
        sb.AppendLine();

        foreach (var prop in entity.CreateProperties)
        {
            GenerateValidationRules(sb, prop, "command");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateUpdateValidator(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// FluentValidation validator for Update{entity.Name}Command");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class Update{entity.Name}CommandValidator : AbstractValidator<Update{entity.Name}Command>");
        sb.AppendLine("{");
        sb.AppendLine($"    public Update{entity.Name}CommandValidator()");
        sb.AppendLine("    {");
        sb.AppendLine("        RuleFor(x => x.Id)");
        sb.AppendLine("            .NotEmpty()");
        sb.AppendLine("            .WithMessage(\"Id is required\");");
        sb.AppendLine();

        foreach (var prop in entity.UpdateProperties)
        {
            GenerateValidationRules(sb, prop, "command");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private void GenerateValidationRules(StringBuilder sb, EntityProperty prop, string prefix)
    {
        var propAccess = $"{prefix}.{ToCamelCase(prop.Name)}";

        if (prop.IsNullable || !prop.IsRequired)
        {
            sb.AppendLine($"        RuleFor(x => x.{ToCamelCase(prop.Name)})");
            sb.AppendLine("            .NotEmpty()");
            sb.AppendLine($"            .When(x => x.{ToCamelCase(prop.Name)} != null)");
            sb.AppendLine($"            .WithMessage(\"{prop.Name} cannot be empty when provided\");");
        }
        else
        {
            sb.AppendLine($"        RuleFor(x => x.{ToCamelCase(prop.Name)})");
            sb.AppendLine("            .NotEmpty()");
            sb.AppendLine($"            .WithMessage(\"{prop.Name} is required\");");
        }

        // Add type-specific validation
        if (prop.Type.Equals("string", StringComparison.OrdinalIgnoreCase))
        {
            if (prop.MaxLength.HasValue)
            {
                sb.AppendLine($"        RuleFor(x => x.{ToCamelCase(prop.Name)})");
                sb.AppendLine($"            .MaximumLength({prop.MaxLength})");
                sb.AppendLine($"            .WithMessage(\"{prop.Name} cannot exceed {prop.MaxLength} characters\");");
            }
        }

        if (prop.Type.Equals("Guid", StringComparison.OrdinalIgnoreCase))
        {
            sb.AppendLine($"        RuleFor(x => x.{ToCamelCase(prop.Name)})");
            sb.AppendLine("            .NotEmpty()");
            sb.AppendLine($"            .WithMessage(\"{prop.Name} must be a valid GUID\");");
        }

        sb.AppendLine();
    }

    #endregion

    #region Domain Events

    private string GenerateDomainEvents(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.Append(GenerateCreatedEvent(entity));
        sb.AppendLine();
        sb.Append(GenerateUpdatedEvent(entity));
        sb.AppendLine();
        sb.Append(GenerateDeletedEvent(entity));
        return sb.ToString();
    }

    private string GenerateCreatedEvent(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Domain event raised when a {entity.Name} is created");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed record {entity.Name}CreatedEvent(");
        sb.AppendLine("    Guid Id,");

        var props = entity.CreateProperties.Take(10).ToList();
        for (int i = 0; i < props.Count; i++)
        {
            var prop = props[i];
            var separator = i < props.Count - 1 ? "," : ") : IDomainEvent;";
            sb.AppendLine($"    {prop.GetTypeWithAnnotation()} {ToPascalCase(prop.Name)}{separator}");
        }

        return sb.ToString();
    }

    private string GenerateUpdatedEvent(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Domain event raised when a {entity.Name} is updated");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed record {entity.Name}UpdatedEvent(");
        sb.AppendLine("    Guid Id,");

        var props = entity.UpdateProperties.Take(10).ToList();
        for (int i = 0; i < props.Count; i++)
        {
            var prop = props[i];
            var separator = i < props.Count - 1 ? "," : ") : IDomainEvent;";
            sb.AppendLine($"    {prop.GetTypeWithAnnotation()} {ToPascalCase(prop.Name)}{separator}");
        }

        return sb.ToString();
    }

    private string GenerateDeletedEvent(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Domain event raised when a {entity.Name} is deleted");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed record {entity.Name}DeletedEvent(");
        sb.AppendLine("    Guid Id");

        var props = entity.Properties.Where(p => !p.IsId && !p.IsCollection).Take(5).ToList();
        foreach (var prop in props)
        {
            sb.AppendLine($"    , {prop.GetTypeWithAnnotation()} {ToPascalCase(prop.Name)}");
        }

        sb.AppendLine(") : IDomainEvent;");

        return sb.ToString();
    }

    #endregion

    #region Repository Interface

    private string GenerateRepositoryInterface(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Repository interface for {entity.Name} entity");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public interface I{entity.Name}Repository : IBaseRepository<{entity.Name}>");
        sb.AppendLine("{");
        sb.AppendLine("    // Add entity-specific repository methods here");
        sb.AppendLine("    ");
        sb.AppendLine("    // Example:");
        sb.AppendLine($"    // Task<{entity.Name}?> GetByNameAsync(string name, CancellationToken cancellationToken = default);");
        sb.AppendLine($"    // Task<List<{entity.Name}>> GetActiveAsync(CancellationToken cancellationToken = default);");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Implementation of I{entity.Name}Repository");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {entity.Name}Repository : BaseRepository<{entity.Name}>, I{entity.Name}Repository");
        sb.AppendLine("{");
        sb.AppendLine($"    public {entity.Name}Repository(");
        sb.AppendLine("        ApplicationDbContext context,");
        sb.AppendLine("        IMapper mapper)");
        sb.AppendLine("        : base(context, mapper)");
        sb.AppendLine("    {");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    #endregion

    #region Errors

    private string GenerateErrors(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Error definitions for {entity.Name} domain");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public static class {entity.Name}Errors");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Creates a 'Not Found' error for {entity.Name}");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static Error NotFound(Guid id) =>");
        sb.AppendLine($"        Error.NotFound(");
        sb.AppendLine($"            \"{entity.Name}.NotFound\",");
        sb.AppendLine($"            $\"The {entity.Name} with ID '{{id}}' was not found.\");");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Creates a 'Already Exists' error for {entity.Name}");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static Error AlreadyExists(string identifier) =>");
        sb.AppendLine($"        Error.AlreadyExists(");
        sb.AppendLine($"            \"{entity.Name}.AlreadyExists\",");
        sb.AppendLine($"            $\"A {entity.Name} with identifier '{{identifier}}' already exists.\");");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Creates a 'Deletion Failed' error for {entity.Name}");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static Error DeletionFailed(Guid id) =>");
        sb.AppendLine($"        Error.Failure(");
        sb.AppendLine($"            \"{entity.Name}.DeletionFailed\",");
        sb.AppendLine($"            $\"Failed to delete the {entity.Name} with ID '{{id}}'.\");");
        sb.AppendLine("}");

        return sb.ToString();
    }

    #endregion

    #region EF Core Configuration

    private string GenerateEfConfiguration(EntityInfo entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"using Domain.Aggregates.{entity.Name}s;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;");
        sb.AppendLine();
        sb.AppendLine($"namespace Infrastructure.Persistence.Configurations;");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Entity Framework Core configuration for {entity.Name}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {entity.Name}Configuration : IEntityTypeConfiguration<{entity.Name}>");
        sb.AppendLine("{");
        sb.AppendLine("    public void Configure(EntityTypeBuilder<{entity.Name}> builder)");
        sb.AppendLine("    {");
        sb.AppendLine($"        builder.ToTable(\"{entity.Name}s\");");
        sb.AppendLine();
        sb.AppendLine("        // Primary Key");
        sb.AppendLine($"        builder.HasKey(e => e.Id);");
        sb.AppendLine();

        // Generate property configurations
        foreach (var prop in entity.Properties.Where(p => !p.IsCollection))
        {
            sb.AppendLine();
            sb.AppendLine($"        // {prop.Name} property");
            sb.AppendLine($"        builder.Property(e => e.{prop.Name})");

            if (prop.IsRequired && !prop.IsNullable)
            {
                sb.AppendLine("            .IsRequired()");
            }

            if (prop.Type.Equals("string", StringComparison.OrdinalIgnoreCase) && prop.MaxLength.HasValue)
            {
                sb.AppendLine($"            .HasMaxLength({prop.MaxLength})");
            }

            if (prop.Type.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("            .IsUnicode(true)");
            }
        }

        // Generate navigation property configurations
        foreach (var navProp in entity.NavigationProperties)
        {
            sb.AppendLine();
            if (navProp.IsCollection)
            {
                sb.AppendLine($"        // {navProp.Name} collection navigation");
                sb.AppendLine($"        builder.HasMany(e => e.{navProp.Name})");
                sb.AppendLine($"            .WithOne()");
                sb.AppendLine("            .HasForeignKey()");
                sb.AppendLine("            .OnDelete(DeleteBehavior.Cascade);");
            }
            else
            {
                sb.AppendLine($"        // {navProp.Name} navigation");
                sb.AppendLine($"        builder.HasOne(e => e.{navProp.Name})");
                sb.AppendLine("            .WithMany()");
                sb.AppendLine("            .HasForeignKey()");
                sb.AppendLine("            .OnDelete(DeleteBehavior.Restrict);");
            }
        }

        // Generate indexes
        sb.AppendLine();
        sb.AppendLine("        // Indexes");
        sb.AppendLine($"        builder.HasIndex(e => e.Id);");

        foreach (var prop in entity.Properties.Where(p => !p.IsCollection && !p.Type.Equals("string")))
        {
            sb.AppendLine($"        builder.HasIndex(e => e.{prop.Name});");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    #endregion

    #region Helper Methods

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length == 1)
            return name.ToLowerInvariant();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length == 1)
            return name.ToUpperInvariant();
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    #endregion
}
