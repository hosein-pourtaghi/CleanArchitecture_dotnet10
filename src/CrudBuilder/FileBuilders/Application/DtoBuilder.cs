using System.Globalization;
using System.Linq;

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
        var reader = new CrudBuilder.EntityReader();
        reader.ReadEntityFile();

        // Build DTO properties
        var dtoProps = reader.Properties
            .Select(p => $"public {p.Type}{(p.IsNullable ? "?" : string.Empty)} {p.Name} {{ get; set; }}")
            .ToList();

        var propsSection = dtoProps.Count > 0 ? "    " + string.Join("\n    ", dtoProps) + "\n" : string.Empty;

        var str =
@$"using System;

namespace Application.Common.DTOs;

public class {MyPath.EntityName}Dto
{{
{propsSection}}}
";

        return str;
    }
     #endregion 


}
