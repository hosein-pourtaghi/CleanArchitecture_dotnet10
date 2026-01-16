using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class GetByIdBuilder
{
    #region GetById
    internal static void GetByIdCommandBuilder(string EntityName)
    {
        Console.WriteLine($"Starting {nameof(GetByIdCommandBuilder)}");

        var createPath = $"{BuilderPath.ApplicationPath}{EntityName}s\\GetById";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetById{EntityName}Command.cs");
        var ss = GetByIdCommandFileBuilder(EntityName);
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetByIdCommandBuilder)} Done");
    }

    internal static void GetByIdCommandHandlerBuilder(string EntityName)
    {
        Console.WriteLine($"Starting {nameof(GetByIdCommandHandlerBuilder)}");

        var createPath = $"{BuilderPath.ApplicationPath}{EntityName}s\\GetById";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetById{EntityName}CommandHandler.cs");
        var ss = GetByIdCommandFileBuilder(EntityName);
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetByIdCommandHandlerBuilder)} Done");
    }

    internal static string GetByIdCommandFileBuilder(string EntityName)
    {
        var str =
@$"
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Domain.{EntityName}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{EntityName}s.GetById;
 
internal sealed class GetGetById{EntityName}QueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetGetById{EntityName}Query, {EntityName}Dto>
{{
    public async Task<Result<{EntityName}Dto>> Handle(GetGetById{EntityName}Query query, CancellationToken cancellationToken)
    {{
        var {EntityName.ToLower(CultureInfo.CurrentCulture)} = await context.{EntityName}s
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        if ({EntityName.ToLower(CultureInfo.CurrentCulture)} is null)
        {{
            return Result.Failure<{EntityName}Dto>({EntityName}Errors.NotFound(query.Id));
        }}

        var {EntityName.ToLower(CultureInfo.CurrentCulture)}Dto = mapper.Map<{EntityName}Dto>({EntityName.ToLower(CultureInfo.CurrentCulture)});

        return {EntityName.ToLower(CultureInfo.CurrentCulture)}Dto;
    }}
}}

";

        return str;
    }
    #endregion 


}
