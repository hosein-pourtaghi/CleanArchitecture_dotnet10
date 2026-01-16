using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class GetAllBuilder
{
    #region GetAll
    internal static void GetAllCommandBuilder(string EntityName)
    {
        Console.WriteLine($"Starting {nameof(GetAllCommandBuilder)}");

        var createPath = $"{BuilderPath.ApplicationPath}{EntityName}s\\GetAll";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetAll{EntityName}Command.cs");
        var ss = GetAllCommandFileBuilder(EntityName);
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetAllCommandBuilder)} Done");
    }

    internal static void GetAllCommandHandlerBuilder(string EntityName)
    {
        Console.WriteLine($"Starting {nameof(GetAllCommandHandlerBuilder)}");

        var createPath = $"{BuilderPath.ApplicationPath}{EntityName}s\\GetAll";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetAll{EntityName}CommandHandler.cs");
        var ss = GetAllCommandHandlerFileBuilder(EntityName);
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetAllCommandHandlerBuilder)} Done");
    }

    internal static string GetAllCommandFileBuilder(string EntityName)
    {
        var str =
@$"
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.{EntityName}s.GetAll;
 
public sealed record GetAll{EntityName}Query() : IQuery<List<{EntityName}Dto>>;

";

        return str;
    }
    internal static string GetAllCommandHandlerFileBuilder(string EntityName)
    {
        var str =
@$"using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{EntityName}s.GetAll;
 
internal sealed class GetAll{EntityName}sQueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetAll{EntityName}sQuery, List<{EntityName}Dto>>
{{
    public async Task<Result<List<{EntityName}Dto>>> Handle(GetAll{EntityName}Query query, CancellationToken cancellationToken)
    {{
        var {EntityName.ToLower(CultureInfo.CurrentCulture)}s = await context.{EntityName}s
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var {EntityName.ToLower(CultureInfo.CurrentCulture)}Dtos = mapper.Map<List<{EntityName}Dto>>({EntityName.ToLower(CultureInfo.CurrentCulture)}s);

        return {EntityName.ToLower(CultureInfo.CurrentCulture)}Dtos;
    }}
}}

";

        return str;
    }
    #endregion 


}
