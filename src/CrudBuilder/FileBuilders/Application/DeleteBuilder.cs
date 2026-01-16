using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class DeleteBuilder
{
    #region Delete
    internal static void DeleteCommandBuilder(string EntityName)
    {
        Console.WriteLine($"Starting {nameof(DeleteCommandBuilder)}");

        var createPath = $"{BuilderPath.ApplicationPath}{EntityName}s\\Delete";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Delete{EntityName}Command.cs");
        var ss = DeleteCommandFileBuilder(EntityName);
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(DeleteCommandBuilder)} Done");
    }

    internal static void DeleteCommandHandlerBuilder(string EntityName)
    {
        Console.WriteLine($"Starting {nameof(DeleteCommandHandlerBuilder)}");

        var createPath = $"{BuilderPath.ApplicationPath}{EntityName}s\\Delete";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Delete{EntityName}CommandHandler.cs");
        var ss = DeleteCommandHandlerFileBuilder(EntityName);
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(DeleteCommandHandlerBuilder)} Done");
    }

    internal static string DeleteCommandFileBuilder(string EntityName)
    {
        var str =
@$"
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.{EntityName}s.Delete;
 
public sealed record Delete{EntityName}Command(Guid Id) : ICommand;
";

        return str;
    }
    internal static string DeleteCommandHandlerFileBuilder(string EntityName)
    {
        var str =
@$"using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.{EntityName}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{EntityName}s.Delete;
 
internal sealed class Delete{EntityName}CommandHandler(IApplicationDbContext context)
    : ICommandHandler<Delete{EntityName}Command>
{{
    public async Task<Result> Handle(Delete{EntityName}Command command, CancellationToken cancellationToken)
    {{
        var {EntityName.ToLower(CultureInfo.CurrentCulture)} = await context.{EntityName}s.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if ({EntityName.ToLower(CultureInfo.CurrentCulture)} is null)
        {{
            return Result.Failure({EntityName}Errors.NotFound(command.Id));
        }}

        // Capture {EntityName.ToLower(CultureInfo.CurrentCulture)} data before deletion for event publishing
        //var deletedEvent = new {EntityName}DeletedDomainEvent(
        //    {EntityName.ToLower(CultureInfo.CurrentCulture)}Id: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Id,
        //    name: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Name,
        //    email: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Email,
        //    phone: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Phone,
        //    address: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Address);
        //{EntityName.ToLower(CultureInfo.CurrentCulture)}.Raise(deletedEvent);

        // Remove from database
        context.{EntityName}s.Remove({EntityName.ToLower(CultureInfo.CurrentCulture)});
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }}
}}
";

        return str;
    }
    #endregion 


}
