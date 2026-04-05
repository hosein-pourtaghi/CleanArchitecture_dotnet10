using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class UpdateBuilder
{
    #region Update
    internal static void UpdateCommandBuilder()
    {
        Console.WriteLine($"Starting {nameof(UpdateCommandBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\Update";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Update{MyPath.EntityName}Command.cs");
        var ss = UpdateCommandFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(UpdateCommandBuilder)} Done");
    }

    internal static void UpdateCommandHandlerBuilder()
    {
        Console.WriteLine($"Starting {nameof(UpdateCommandHandlerBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\Update";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Update{MyPath.EntityName}CommandHandler.cs");
        var ss = UpdateCommandHandlerFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(UpdateCommandHandlerBuilder)} Done");
    }

    internal static string UpdateCommandFileBuilder()
    {
        var reader = new CrudBuilder.EntityReader();
        reader.ReadEntityFile();

        var parameters = reader.UpdateCommandParameters;
        var paramSection = string.IsNullOrWhiteSpace(parameters) ? string.Empty : $"\n    {parameters}\n";

        var str =
@$"using Application.Common.Messaging;
using Application.Common.DTOs;

namespace  Application.{MyPath.EntityName}s.Update;

public sealed record Update{MyPath.EntityName}Command({paramSection}) : ICommand; 
";

        return str;
    }
    internal static string UpdateCommandHandlerFileBuilder()
    {
        var reader = new CrudBuilder.EntityReader();
        reader.ReadEntityFile();

        //var assignments = reader.PropertyAssignments;

        var str =
@$"
using Application.Common.Data;
using Application.Common.Messaging;
using AutoMapper;
using Domain.{MyPath.EntityName}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{MyPath.EntityName}s.Update;
 
internal sealed class Update{MyPath.EntityName}CommandHandler(
    IApplicationDbContext context,
    IMapper mapper
    )
    : ICommandHandler<Update{MyPath.EntityName}Command>
{{
    public async Task<Result> Handle(Update{MyPath.EntityName}Command command, CancellationToken cancellationToken)
    {{
        var {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} = await context.{MyPath.EntityName}s.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if ({MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} is null)
        {{
            return Result.Failure({MyPath.EntityName}Errors.NotFound(command.Id));
        }}

        // Update entity with new values 
        {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} = mapper.Map<{MyPath.EntityName}>(command);


        //// Publish comprehensive domain event with all updated data for auditing and message bus
        //{MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Raise(new {MyPath.EntityName}UpdatedDomainEvent(
        //    {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}Id: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Id,
        //    name: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Name,
        //    email: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Email,
        //    phone: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Phone,
        //    address: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Address));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }}
}}

";

        return str;
    }
    #endregion 


}
