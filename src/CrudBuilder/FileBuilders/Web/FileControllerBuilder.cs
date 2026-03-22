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
public class {MyPath.EntityName}sController(IMediator mediator) : ApiController
{{
    
    [HttpGet]
    [ProducesResponseType(typeof(List<{MyPath.EntityName}Dto>), StatusCodes.Status200OK)]                               
    [Produces(""application/json"")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {{
        var result = await mediator.Send(new GetAll{MyPath.EntityName}Query(), cancellationToken);
        return HandleResult(result);
    }}
 
    [HttpGet(""{{id:guid}}"")]
    [ProducesResponseType(typeof({MyPath.EntityName}Dto), StatusCodes.Status200OK)]                               
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {{
        var result = await mediator.Send(new GetById{MyPath.EntityName}Query(id), cancellationToken);
        return HandleResult(result);
    }}
 
    [HttpPost]                               
    public async Task<IActionResult> Create(
        [FromBody] Create{MyPath.EntityName}Command command,
        CancellationToken cancellationToken)
    {{ 
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {{
            return HandleResult<Guid>(result);
        }}

        return CreatedAtAction(nameof(GetById), new {{ id = result.Value }}, new {{ id = result.Value }});
    }}

    [HttpPut(""{{id:guid}}"")]                               
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] Update{MyPath.EntityName}Command command,
        CancellationToken cancellationToken)
    {{ 
        var result = await mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }}

    [HttpDelete(""{{id:guid}}"")]                               
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {{
        var result = await mediator.Send(new Delete{MyPath.EntityName}Command(id), cancellationToken);
        return HandleResult(result);
    }} 

}}
 
";

        return str;
    }
    #endregion

}
