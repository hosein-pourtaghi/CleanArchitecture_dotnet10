using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class DeleteBuilder
{
    #region Delete
    internal static void DeleteCommandBuilder()
    {
        Console.WriteLine($"Starting {nameof(DeleteCommandBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\Delete";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Delete{MyPath.EntityName}Command.cs");
        var ss = DeleteCommandFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(DeleteCommandBuilder)} Done");
    }

    internal static void DeleteCommandHandlerBuilder()
    {
        Console.WriteLine($"Starting {nameof(DeleteCommandHandlerBuilder)}");

        var createPath = $"{MyPath.ApplicationPath}{MyPath.EntityName}s\\Delete";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Delete{MyPath.EntityName}CommandHandler.cs");
        var ss = DeleteCommandHandlerFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(DeleteCommandHandlerBuilder)} Done");
    }

    internal static string DeleteCommandFileBuilder()
    {
        var str =
@$"
using Application.Common.Messaging;
using Application.Common.DTOs;

namespace  Application.{MyPath.EntityName}s.Delete;
 
public sealed record Delete{MyPath.EntityName}Command(Guid Id) : ICommand;
";

        return str;
    }
    internal static string DeleteCommandHandlerFileBuilder()
    {
        var str =
@$"using Application.Common.Data;
using Application.Common.Messaging;
using Domain.{MyPath.EntityName}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{MyPath.EntityName}s.Delete;
 
internal sealed class Delete{MyPath.EntityName}CommandHandler(IApplicationDbContext context)
    : ICommandHandler<Delete{MyPath.EntityName}Command>
{{
    public async Task<Result> Handle(Delete{MyPath.EntityName}Command command, CancellationToken cancellationToken)
    {{
        var {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} = await context.{MyPath.EntityName}s.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if ({MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} is null)
        {{
            return Result.Failure({MyPath.EntityName}Errors.NotFound(command.Id));
        }}

        // Capture {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)} data before deletion for event publishing
        //var deletedEvent = new {MyPath.EntityName}DeletedDomainEvent(
        //    {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}Id: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Id,
        //    name: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Name,
        //    email: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Email,
        //    phone: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Phone,
        //    address: {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Address);
        //{MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.Raise(deletedEvent);

        // Remove from database
        context.{MyPath.EntityName}s.Remove({MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)});
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }}
}}
";

        return str;
    }
    #endregion 


}
