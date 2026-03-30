using Application.Checklists.Create;
using Application.Checklists.Delete;
using Application.Checklists.GetAll;
using Application.Checklists.GetById;
using Application.Checklists.Update;
using Application.Common.DTOs;
using Application.Common.DTOs.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class ChecklistsController(IMediator mediator) : ApiController
{

    [HttpPost]
    [ProducesResponseType(typeof(List<ChecklistDto>), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> GetAll([FromBody] PaginatedRequest filter)
    {
        // Client uses filter parameter for ALL conditions:
        // ?filter=IsActive,true,Equal,And&filter=Title,Security,Contains,And
        var result = await mediator.Send(new GetAllChecklistQuery(filter));
        return HandleResult(result);
        //return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ChecklistDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetByIdChecklistQuery(id), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateChecklistCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleResult<Guid>(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateChecklistCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteChecklistCommand(id), cancellationToken);
        return HandleResult(result);
    }

}

