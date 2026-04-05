using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class GetByIdBuilder
{
    #region GetById
    internal static void GetByIdQueryBuilder()
    {
        Console.WriteLine($"Starting {nameof(GetByIdQueryBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\GetById";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetById{MyPath.EntityName}Query.cs");
        var ss = GetByIdQueryFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetByIdQueryBuilder)} Done");
    }

    internal static string GetByIdQueryFileBuilder()
    {
        var str =
@$"using Application.Common.Messaging;
using Application.Common.DTOs;

namespace Application.{MyPath.EntityName}s.GetById;

/// <summary>
/// Query to retrieve a {MyPath.EntityName} by its unique identifier.
/// Inherits from IQuery&lt;{MyPath.EntityName}Dto&gt; which returns Result&lt;{MyPath.EntityName}Dto&gt;.
/// Handled by <see cref=""GetById{MyPath.EntityName}QueryHandler""/>
/// </summary>
public sealed record GetById{MyPath.EntityName}Query(Guid Id) : IQuery<{MyPath.EntityName}Dto>;
";

        return str;
    }

    internal static void GetByIdQueryHandlerBuilder()
    {
        Console.WriteLine($"Starting {nameof(GetByIdQueryHandlerBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\GetById";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetById{MyPath.EntityName}QueryHandler.cs");
        var ss = GetByIdQueryHandlerFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetByIdQueryHandlerBuilder)} Done");
    }

    internal static string GetByIdQueryHandlerFileBuilder()
    {
        var str =
@$"using Application.Common.Data;
using Application.Common.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Domain.{MyPath.EntityName}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{MyPath.EntityName}s.GetById;

/// <summary>
/// Handles <see cref=""GetById{MyPath.EntityName}Query""/> requests.
/// Retrieves a single {MyPath.EntityName} by ID with no-tracking for read-only access.
/// Returns <see cref=""Result{{TValue}}""/> containing the DTO or a failure result if not found.
/// </summary>
internal sealed class GetById{MyPath.EntityName}QueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetById{MyPath.EntityName}Query, {MyPath.EntityName}Dto>
{{
    public async Task<Result<{MyPath.EntityName}Dto>> Handle(
        GetById{MyPath.EntityName}Query query,
        CancellationToken cancellationToken)
    {{
        var {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} = await context.{MyPath.EntityName}s
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        if ({MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} is null)
        {{
            return Result.Failure<{MyPath.EntityName}Dto>(
                {MyPath.EntityName}Errors.NotFound(query.Id));
        }}

        var {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}Dto = mapper.Map<{MyPath.EntityName}Dto>(
            {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)});

        return {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}Dto;
    }}
}}
";

        return str;
    }
    #endregion 


}
