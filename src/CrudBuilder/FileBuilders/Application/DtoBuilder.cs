using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class DtoBuilder
{
    #region Dto
    internal static void BuildDto()
    {
        Console.WriteLine($"Starting {nameof(BuildDto)}");

        var dtoPath = $"{MyPath.DtoPath}";
        var dir = Directory.CreateDirectory(dtoPath);
        using var file = File.OpenWrite($"{dtoPath}\\{MyPath.EntityName}Dto.cs");
        var ss = DtoFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(BuildDto)} Done");
    }
 
    internal static string DtoFileBuilder()
    {
        var str =
@$"
using Application.Common.DTOs;
using AutoDto;
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
