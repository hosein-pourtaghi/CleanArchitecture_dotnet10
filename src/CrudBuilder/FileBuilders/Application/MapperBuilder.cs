using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class MapperBuilder
{
    #region Mapper
    internal static void BuildMapper()
    {
        Console.WriteLine($"Starting {nameof(BuildMapper)}");

        var mapperPath = $"{MyPath.MapperPath}";
        var dir = Directory.CreateDirectory(mapperPath);
        using var file = File.OpenWrite($"{mapperPath}\\{MyPath.EntityName}Profile.cs");
        var ss = MapperFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(BuildMapper)} Done");
    }
 
    internal static string MapperFileBuilder()
    {
        var str =
@$"
using Application.Common.DTOs;
using AutoMapper;
using Domain.{MyPath.EntityName}s;

namespace Application.Common.Mappings;
 
public class {MyPath.EntityName}Profile : Profile
{{
    public {MyPath.EntityName}Profile()
    {{

        CreateMap<{MyPath.EntityName}, {MyPath.EntityName}Dto>()
            ;


    }}
}}

";

        return str;
    }
     #endregion 


}
