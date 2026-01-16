using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class UpdateBuilder
{
    #region Update
    internal static void UpdateCommandBuilder(string EntityName)
    {
        Console.WriteLine($"Starting {nameof(UpdateCommandBuilder)}");

        var createPath = $"{BuilderPath.ApplicationPath}{EntityName}s\\Update";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Update{EntityName}Command.cs");
        var ss = UpdateCommandFileBuilder(EntityName);
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(UpdateCommandBuilder)} Done");
    }

    internal static void UpdateCommandHandlerBuilder(string EntityName)
    {
        Console.WriteLine($"Starting {nameof(UpdateCommandHandlerBuilder)}");

        var createPath = $"{BuilderPath.ApplicationPath}{EntityName}s\\Update";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Update{EntityName}CommandHandler.cs");
        var ss = UpdateCommandHandlerFileBuilder(EntityName);
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(UpdateCommandHandlerBuilder)} Done");
    }

    internal static string UpdateCommandFileBuilder(string EntityName)
    {
        var str =
@$"
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.{EntityName}s.Update;

public sealed record Update{EntityName}Command(
    ) : ICommand<Guid>; 
";

        return str;
    }
    internal static string UpdateCommandHandlerFileBuilder(string EntityName)
    {
        var str =
@$"
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using AutoMapper;
using Domain.{EntityName}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{EntityName}s.Update;
 
internal sealed class Update{EntityName}CommandHandler(
    IApplicationDbContext context,
    IMapper mapper
    )
    : ICommandHandler<Update{EntityName}Command>
{{
    public async Task<Result> Handle(Update{EntityName}Command command, CancellationToken cancellationToken)
    {{
        var {EntityName.ToLower(CultureInfo.CurrentCulture)} = await context.{EntityName}s.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if ({EntityName.ToLower(CultureInfo.CurrentCulture)} is null)
        {{
            return Result.Failure({EntityName}Errors.NotFound(command.Id));
        }}

        // Update entity with new values 
        {EntityName.ToLower(CultureInfo.CurrentCulture)} = mapper.Map<{EntityName}>(command);


        //// Publish comprehensive domain event with all updated data for auditing and message bus
        //{EntityName.ToLower(CultureInfo.CurrentCulture)}.Raise(new {EntityName}UpdatedDomainEvent(
        //    {EntityName.ToLower(CultureInfo.CurrentCulture)}Id: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Id,
        //    name: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Name,
        //    email: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Email,
        //    phone: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Phone,
        //    address: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Address));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }}
}}

";

        return str;
    }
    #endregion 


}
