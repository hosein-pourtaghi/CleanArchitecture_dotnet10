using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class GetByIdBuilder
{
    #region GetById
    internal static void GetByIdCommandBuilder()
    {
        Console.WriteLine($"Starting {nameof(GetByIdCommandBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\GetById";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetById{MyPath.EntityName}Command.cs");
        var ss = GetByIdCommandFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetByIdCommandBuilder)} Done");
    }

    internal static void GetByIdCommandHandlerBuilder()
    {
        Console.WriteLine($"Starting {nameof(GetByIdCommandHandlerBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\GetById";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetById{MyPath.EntityName}CommandHandler.cs");
        var ss = GetByIdCommandFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetByIdCommandHandlerBuilder)} Done");
    }

    internal static string GetByIdCommandFileBuilder()
    {
        var str =
@$"
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Domain.{MyPath.EntityName}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{MyPath.EntityName}s.GetById;
 
internal sealed class GetById{MyPath.EntityName}QueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetById{MyPath.EntityName}Query, {MyPath.EntityName}Dto>
{{
    public async Task<Result<{MyPath.EntityName}Dto>> Handle(GetById{MyPath.EntityName}Query query, CancellationToken cancellationToken)
    {{
        var {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} = await context.{MyPath.EntityName}s
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        if ({MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} is null)
        {{
            return Result.Failure<{MyPath.EntityName}Dto>({MyPath.EntityName}Errors.NotFound(query.Id));
        }}

        var {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}Dto = mapper.Map<{MyPath.EntityName}Dto>({MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)});

        return {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}Dto;
    }}
}}

";

        return str;
    }
    #endregion 


}
