using System.Globalization;

namespace CrudBuilder.FileBuilders.Application;
internal static class CreateBuilder
{
    #region Create
    internal static void CreateCommandBuilder(string EntityName)
    {
        Console.WriteLine($"Starting {nameof(CreateCommandBuilder)}");

        var createPath = $"{BuilderPath.ApplicationPath}{EntityName}s\\Create";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Create{EntityName}Command.cs");
        var ss = CreateCommandFileBuilder(EntityName);
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(CreateCommandBuilder)} Done");
    }
    internal static void CreateCommandHandlerBuilder(string EntityName)
    {
        Console.WriteLine($"Starting {nameof(CreateCommandHandlerBuilder)}");

        var createPath = $"{BuilderPath.ApplicationPath}{EntityName}s\\Create";
        var dir = Directory.CreateDirectory(createPath);
        using var file = File.OpenWrite($"{createPath}\\Create{EntityName}CommandHandler.cs");
        var ss = CreateCommandHandlerFileBuilder(EntityName);
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(CreateCommandHandlerBuilder)} Done");
    }

    internal static string CreateCommandFileBuilder(string EntityName)
    {
        var str =
@$"
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.{EntityName}s.Create;

public sealed record Create{EntityName}Command(
    ) : ICommand<Guid>; 
";

        return str;
    }
    internal static string CreateCommandHandlerFileBuilder(string EntityName)
    {
        var str =
@$"
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using AutoMapper;
using Domain.{EntityName}s;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.{EntityName}s.Create;

internal sealed class Create{EntityName}CommandHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : ICommandHandler<Create{EntityName}Command, Guid>
{{
    public async Task<Result<Guid>> Handle(Create{EntityName}Command command, CancellationToken cancellationToken)
    {{ 
        // Create {EntityName.ToLower(CultureInfo.CurrentCulture)} entity
        var {EntityName.ToLower(CultureInfo.CurrentCulture)} = mapper.Map<{EntityName}>(command); 


        // Publish comprehensive domain event for audit logging and async operations (message bus)
        //{EntityName.ToLower(CultureInfo.CurrentCulture)}.Raise(new {EntityName}CreatedDomainEvent(
        //    {EntityName.ToLower(CultureInfo.CurrentCulture)}Id: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Id,
        //    name: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Name,
        //    email: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Email,
        //    phone: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Phone,
        //    address: {EntityName.ToLower(CultureInfo.CurrentCulture)}.Address));

        // Persist to database
        context.{EntityName}s.Add({EntityName.ToLower(CultureInfo.CurrentCulture)});
        await context.SaveChangesAsync(cancellationToken);

        return {EntityName.ToLower(CultureInfo.CurrentCulture)}.Id;
    }}
}}

";

        return str;
    }
    #endregion

}
