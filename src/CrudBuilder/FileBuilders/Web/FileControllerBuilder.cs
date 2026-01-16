using System.Globalization;

namespace CrudBuilder.FileBuilders.Web;
internal static class FileControllerBuilder
{
    #region Create Controller
    internal static void CreateController()
    {
        Console.WriteLine($"Starting {nameof(CreateController)}");

        var controllerPath = $"{MyPath.PersistancePath}";
        var dir = Directory.CreateDirectory(controllerPath);
        using var file = File.OpenWrite($"{controllerPath}\\{MyPath.EntityName}sController.cs");
        var ss = CreateControllerFileBuilder();
        var byt = System.Text.Encoding.UTF8.GetBytes(ss.ToString());
        if (byt != null)
        {
            file.Write(byt);
        }
        file.Close();
        file.Dispose();

        Console.WriteLine($"{nameof(CreateController)} Done");
    }

    internal static string CreateControllerFileBuilder()
    {
        var str =
@$"using Application.Abstractions.Messaging;
using Application.Common.DTOs;
using Application.{MyPath.EntityName}s.Copy;
using Application.{MyPath.EntityName}s.Create;
using Application.{MyPath.EntityName}s.Delete;
using Application.{MyPath.EntityName}s.Get;
using Application.{MyPath.EntityName}s.GetById;
using Application.{MyPath.EntityName}s.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
 
[Route(""api/[controller]/[action]"")]
[ApiController]
[Authorize] 
public class {MyPath.EntityName}sController(
    ICommandHandler<Create{MyPath.EntityName}Command, Guid> createCommandHandler, 
    IQueryHandler<Get{MyPath.EntityName}sQuery, List<{MyPath.EntityName}Dto>> get{MyPath.EntityName}sQueryHandler,
    IQueryHandler<Get{MyPath.EntityName}ByIdQuery, {MyPath.EntityName}Dto> get{MyPath.EntityName}ByIdQueryHandler,
    ICommandHandler<Update{MyPath.EntityName}Command> updateCommandHandler,
    ICommandHandler<Delete{MyPath.EntityName}Command> deleteCommandHandler) : ApiController
{{
    
    [HttpGet]
    [ProducesResponseType(typeof(List<{MyPath.EntityName}Dto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces(""application/json"")]
    public async Task<IActionResult> Get{MyPath.EntityName}s(CancellationToken cancellationToken)
    {{
        var result = await get{MyPath.EntityName}sQueryHandler.Handle(new Get{MyPath.EntityName}sQuery(), cancellationToken);
        return HandleResult(result);
    }}
 
    [HttpGet(""{{id:guid}}"")]
    [ProducesResponseType(typeof({MyPath.EntityName}Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {{
        var result = await get{MyPath.EntityName}ByIdQueryHandler.Handle(new Get{MyPath.EntityName}ByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }}
 
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] Create{MyPath.EntityName}Request request,
        CancellationToken cancellationToken)
    {{
        var command = new Create{MyPath.EntityName}Command(
            request.Name,
            request.Email,
            request.Phone,
            request.Address);

        var result = await createCommandHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {{
            return HandleResult<Guid>(result);
        }}

        return CreatedAtAction(nameof(GetById), new {{ id = result.Value }}, new {{ id = result.Value }});
    }}

    [HttpPut(""{{id:guid}}"")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] Update{MyPath.EntityName}Request request,
        CancellationToken cancellationToken)
    {{
        var command = new Update{MyPath.EntityName}Command(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.Address);

        var result = await updateCommandHandler.Handle(command, cancellationToken);
        return HandleResult(result);
    }}

    [HttpDelete(""{{id:guid}}"")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {{
        var result = await deleteCommandHandler.Handle(new Delete{MyPath.EntityName}Command(id), cancellationToken);
        return HandleResult(result);
    }} 

}}

 

/// <summary>
/// Request model for creating a new {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.
/// </summary>
/// <remarks>
/// All fields follow clean data requirements:
/// - No HTML/script injection
/// - SQL injection prevention
/// - Proper encoding for special characters
/// </remarks>
public sealed class Create{MyPath.EntityName}Request
{{
    /// <summary>{MyPath.EntityName}'s full name (required). Must be 1-200 characters.</summary>
    /// <example>John Doe</example>
    public required string Name {{ get; set; }}

    /// <summary>{MyPath.EntityName}'s email address (required). Must be unique and valid email format.</summary>
    /// <example>john.doe@example.com</example>
    public required string Email {{ get; set; }}

    /// <summary>{MyPath.EntityName}'s phone number (optional). Maximum 20 characters.</summary>
    /// <example>+1-555-0123</example>
    public string? Phone {{ get; set; }}

    /// <summary>{MyPath.EntityName}'s physical address (optional). Maximum 500 characters.</summary>
    /// <example>123 Main Street, Anytown, ST 12345</example>
    public string? Address {{ get; set; }}
}}

public sealed class Copy{MyPath.EntityName}Request
{{
    /// <summary>{MyPath.EntityName}'s full name (required). Must be 1-200 characters.</summary>
    /// <example>John Doe</example>
    public required string Name {{ get; set; }}

    /// <summary>{MyPath.EntityName}'s email address (required). Must be unique and valid email format.</summary>
    /// <example>john.doe@example.com</example>
    public required string Email {{ get; set; }}

    /// <summary>{MyPath.EntityName}'s phone number (optional). Maximum 20 characters.</summary>
    /// <example>+1-555-0123</example>
    public string? Phone {{ get; set; }}

    /// <summary>{MyPath.EntityName}'s physical address (optional). Maximum 500 characters.</summary>
    /// <example>123 Main Street, Anytown, ST 12345</example>
    public string? Address {{ get; set; }}
}}

/// <summary>
/// Request model for updating an existing {MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.
/// </summary>
/// <remarks>
/// All fields are required when updating. To keep a field unchanged, provide its current value.
/// </remarks>
public sealed class Update{MyPath.EntityName}Request
{{
    /// <summary>{MyPath.EntityName}'s full name (required). Must be 1-200 characters.</summary>
    /// <example>Jane Doe</example>
    public required string Name {{ get; set; }}

    /// <summary>{MyPath.EntityName}'s email address (required). Must be valid email format.</summary>
    /// <example>jane.doe@example.com</example>
    public required string Email {{ get; set; }}

    /// <summary>{MyPath.EntityName}'s phone number (optional). Maximum 20 characters.</summary>
    /// <example>+1-555-0456</example>
    public string? Phone {{ get; set; }}

    /// <summary>{MyPath.EntityName}'s physical address (optional). Maximum 500 characters.</summary>
    /// <example>456 Oak Avenue, Somewhere, ST 54321</example>
    public string? Address {{ get; set; }}
}}

";

        return str;
    }
    #endregion

}
