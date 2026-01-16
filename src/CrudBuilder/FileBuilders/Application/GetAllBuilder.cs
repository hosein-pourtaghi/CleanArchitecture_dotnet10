using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class GetAllBuilder
{
    #region GetAll
    internal static void GetAllCommandBuilder()
    {
        Console.WriteLine($"Starting {nameof(GetAllCommandBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\GetAll";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetAll{MyPath.EntityName}Command.cs");
        var ss = GetAllCommandFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetAllCommandBuilder)} Done");
    }

    internal static void GetAllCommandHandlerBuilder()
    {
        Console.WriteLine($"Starting {nameof(GetAllCommandHandlerBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\GetAll";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetAll{MyPath.EntityName}CommandHandler.cs");
        var ss = GetAllCommandHandlerFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetAllCommandHandlerBuilder)} Done");
    }

    internal static string GetAllCommandFileBuilder()
    {
        var str =
@$"
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.{MyPath.EntityName}s.GetAll;
 
public sealed record GetAll{MyPath.EntityName}Query() : IQuery<List<{MyPath.EntityName}Dto>>;

";

        return str;
    }
    internal static string GetAllCommandHandlerFileBuilder()
    {
        var str =
@$"using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{MyPath.EntityName}s.GetAll;
 
internal sealed class GetAll{MyPath.EntityName}sQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetAll{MyPath.EntityName}sQuery, List<{MyPath.EntityName}Dto>>
{{
    public async Task<Result<List<{MyPath.EntityName}Dto>>> Handle(GetAll{MyPath.EntityName}Query query, CancellationToken cancellationToken)
    {{
        var {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}s = await context.{MyPath.EntityName}s
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}Dtos = mapper.Map<List<{MyPath.EntityName}Dto>>({MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}s);

        return {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}Dtos;
    }}
}}

";

        return str;
    }
    #endregion 


}
