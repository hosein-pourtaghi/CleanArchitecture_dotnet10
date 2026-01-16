namespace CrudBuilder.FileBuilders.Persistance;
internal static class FileConfigurationBuilder
{
    #region Create Configuration
    internal static void CreateConfiguration()
    {
        Console.WriteLine($"Starting {nameof(CreateConfiguration)}");

        var createPath = $"{MyPath.PersistancePath}";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\{MyPath.EntityName}Configuration.cs");
        var ss = CreateCommandFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(CreateConfiguration)} Done");
    }

    internal static string CreateCommandFileBuilder()
    {
        var str =
@$"
using Domain.{MyPath.EntityName}s;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class {MyPath.EntityName}Configuration : IEntityTypeConfiguration<{MyPath.EntityName}>
{{
    public void Configure(EntityTypeBuilder<{MyPath.EntityName}> builder)
    {{
        //builder.ToTable(""{MyPath.EntityName}s"");

        //builder.HasKey(c => c.Id);

        //builder.Property(c => c.Name)
        //    .IsRequired()
        //    .HasMaxLength(200);

        //builder.Property(c => c.Email)
        //    .IsRequired()
        //    .HasMaxLength(256);

        //builder.HasIndex(c => c.Email)
        //    .IsUnique()
        //    ;
 
    }}
}}

";

        return str;
    }
    #endregion

}
