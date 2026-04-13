using Application.Checklists.Create;
using Application.Checklists.Delete;
using Application.Checklists.GetAll;
using Application.Checklists.GetById;
using Application.Checklists.Update;
using Application.Common.DTOs;
using Application.Common.DTOs.Shared;
using Application.Common.Messaging;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers; 

[Area("Checklist")]
[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class Checklists2Controller(IMediator mediator) :ApiController
    //: base(mediator)
    //:
    //CrudController<CreateChecklistCommand, GetAllChecklistQuery, ChecklistDto>,
    //IHaveGetById
{
    //public Checklists2Controller(IMediator mediator)  { }



    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromBody] PaginatedRequest filter, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllChecklistQuery(filter), ct);
        return HandlePaginatedResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetByIdChecklistQuery(id), ct);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateChecklistCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction("GetById", new { id = result.Value }, new { id = result.Value })
            : HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChecklistCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteChecklistCommand(id), ct);
        return HandleResult(result);
    }





    //protected override IQuery<PaginatedResult<ChecklistDto>> BuildGetAllQuery(PaginatedRequest filter)
    //    => new GetAllChecklistQuery(filter);

    //protected override IQuery BuildGetByIdQuery(Guid id)
    //    => new GetByIdChecklistQuery(id);

    //protected override ICommand BuildCreateCommand(CreateChecklistCommand command)
    //    => command;

    //protected override ICommand BuildUpdateCommand(Guid id, CreateChecklistCommand command)
    //    => new UpdateChecklistCommand(
    //        Id: id,
    //        isActive: command.isActive,
    //        isValid: command.isValid,
    //        title: command.title,
    //        groups: command.groups);

    //protected override ICommand BuildDeleteCommand(Guid id)
    //    => new DeleteChecklistCommand(id);



}
