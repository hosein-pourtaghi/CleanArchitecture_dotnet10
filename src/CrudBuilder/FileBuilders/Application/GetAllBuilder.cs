using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class GetAllBuilder
{
    #region GetAll
    internal static void GetAllQueryBuilder()
    {
        Console.WriteLine($"Starting {nameof(GetAllQueryBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\GetAll";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetAll{MyPath.EntityName}Query.cs");
        var ss = GetAllQueryFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetAllQueryBuilder)} Done");
    }

    internal static void GetAllQueryHandlerBuilder()
    {
        Console.WriteLine($"Starting {nameof(GetAllQueryHandlerBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\GetAll";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\GetAll{MyPath.EntityName}QueryHandler.cs");
        var ss = GetAllQueryHandlerFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(GetAllQueryHandlerBuilder)} Done");
    }

    internal static string GetAllQueryFileBuilder()
    {
        var str =
@$"
using Application.Common.Messaging;
using Application.Common.DTOs;

namespace  Application.{MyPath.EntityName}s.GetAll;
 
public sealed record GetAll{MyPath.EntityName}Query() : IQuery<List<{MyPath.EntityName}Dto>>;

";

        return str;
    }
    internal static string GetAllQueryHandlerFileBuilder()
    {
        var str =
@$"using Application.Common.Data;
using Application.Common.Messaging;
using Application.Common.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{MyPath.EntityName}s.GetAll;
 
internal sealed class GetAll{MyPath.EntityName}QueryHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : IQueryHandler<GetAll{MyPath.EntityName}Query, List<{MyPath.EntityName}Dto>>
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
