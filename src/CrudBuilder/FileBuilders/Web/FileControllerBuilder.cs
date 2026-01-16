using System.Globalization;

namespace CrudBuilder.FileBuilders.Web;
internal static class FileControllerBuilder
{
    #region Create Controller
    internal static void CreateController()
    {
        Console.WriteLine($"Starting {nameof(CreateController)}");

        var controllerPath = $"{MyPath.ControllerPath}";
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
using Application.{MyPath.EntityName}s.Create;
using Application.{MyPath.EntityName}s.Delete;
using Application.{MyPath.EntityName}s.GetAll;
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
    IQueryHandler<GetAll{MyPath.EntityName}Query, List<{MyPath.EntityName}Dto>> getAll{MyPath.EntityName}QueryHandler,
    IQueryHandler<GetById{MyPath.EntityName}Query, {MyPath.EntityName}Dto> getById{MyPath.EntityName}QueryHandler,
    ICommandHandler<Update{MyPath.EntityName}Command> updateCommandHandler,
    ICommandHandler<Delete{MyPath.EntityName}Command> deleteCommandHandler) : ApiController
{{
    
    [HttpGet]
    [ProducesResponseType(typeof(List<{MyPath.EntityName}Dto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces(""application/json"")]
    public async Task<IActionResult> GetAll{MyPath.EntityName}(CancellationToken cancellationToken)
    {{
        var result = await getAll{MyPath.EntityName}QueryHandler.Handle(new GetAll{MyPath.EntityName}Query(), cancellationToken);
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
        var result = await getById{MyPath.EntityName}QueryHandler.Handle(new GetById{MyPath.EntityName}Query(id), cancellationToken);
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
            
        );

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
            request.Id
        );

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

  
public sealed class Create{MyPath.EntityName}Request
{{ 

}}
 
public sealed class Update{MyPath.EntityName}Request
{{
    public Required Guid Id {{ get; set; }}

}}

";

        return str;
    }
    #endregion

}
