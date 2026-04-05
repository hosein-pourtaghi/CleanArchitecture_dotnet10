using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class CreateBuilder
{
    #region Create
    internal static void CreateCommandBuilder()
    {
        Console.WriteLine($"Starting {nameof(CreateCommandBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\Create";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Create{MyPath.EntityName}Command.cs");
        var ss = CreateCommandFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(CreateCommandBuilder)} Done");
    }
    internal static void CreateCommandHandlerBuilder()
    {
        Console.WriteLine($"Starting {nameof(CreateCommandHandlerBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\Create";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Create{MyPath.EntityName}CommandHandler.cs");
        var ss = CreateCommandHandlerFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(CreateCommandHandlerBuilder)} Done");
    }

    internal static string CreateCommandFileBuilder()
    {
        // Read entity properties to build command parameters
        var reader = new CrudBuilder.EntityReader();
        reader.ReadEntityFile();
        var parameters = reader.CreateCommandParameters;

        var paramSection = string.IsNullOrWhiteSpace(parameters) ? string.Empty : $"\n    {parameters}\n";

        var str =
@$"using Application.Common.Messaging;
using Application.Common.DTOs;

namespace  Application.{MyPath.EntityName}s.Create;

public sealed record Create{MyPath.EntityName}Command({paramSection}) : ICommand<Guid>; 
";

        return str;
    }
    internal static string CreateCommandHandlerFileBuilder()
    {
        var str =
@$"
using Application.Common.Data;
using Application.Common.Messaging;
using AutoMapper;
using Domain.{MyPath.EntityName}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{MyPath.EntityName}s.Create;

internal sealed class Create{MyPath.EntityName}CommandHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : ICommandHandler<Create{MyPath.EntityName}Command, Guid>
{{
    public async Task<Result<Guid>> Handle(Create{MyPath.EntityName}Command command, CancellationToken cancellationToken)
    {{ 
        // Create {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} entity
        var {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} = mapper.Map<{MyPath.EntityName}>(command); 


        // Publish comprehensive domain event for audit logging and async operations (message bus)
        //{MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Raise(new {MyPath.EntityName}CreatedDomainEvent(
        //    {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}Id: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Id,
        //    name: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Name,
        //    email: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Email,
        //    phone: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Phone,
        //    address: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Address));

        // Persist to database
        context.{MyPath.EntityName}s.Add({MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)});
        await context.SaveChangesAsync(cancellationToken);

        return {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Id;
    }}
}}

";

        return str;
    }
    #endregion

}
