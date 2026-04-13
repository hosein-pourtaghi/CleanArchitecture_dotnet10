using Application.Common.DTOs.Shared;
using Application.Common.Messaging;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
 

//[ApiController]
//[Authorize]
//public abstract class CrudController<TCommand, TQuery, TDto> : ApiController
//    where TCommand : class
//    where TQuery : class
//{
//    protected readonly IMediator Mediator;

//    protected CrudController(IMediator mediator) => Mediator = mediator;

//    /// <summary>
//    /// Get all with pagination, filtering, sorting
//    /// </summary>
//    [HttpPost]
//    [ProducesResponseType( StatusCodes.Status200OK)]
//    public async Task<IActionResult> GetAll([FromBody] PaginatedRequest filter, CancellationToken ct)
//        => HandlePaginatedResult(await Mediator.Send(BuildGetAllQuery(filter), ct));

//    /// <summary>
//    /// Get by ID
//    /// </summary>
//    [HttpGet("{id:guid}")]
//    [ProducesResponseType(  StatusCodes.Status200OK)]
//    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
//        => HandleResult(await Mediator.Send(BuildGetByIdQuery(id), ct));

//    /// <summary>
//    /// Create new entity
//    /// </summary>
//    [HttpPost]
//    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
//    public async Task<IActionResult> Create([FromBody] TCommand command, CancellationToken ct)
//    {
//        var result = await Mediator.Send(BuildCreateCommand(command), ct);

//        if (result.IsFailure)
//            return HandleResult(result);

//        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value }); 
//    }

//    /// <summary>
//    /// Update existing entity
//    /// </summary>
//    [HttpPut("{id:guid}")]
//    [ProducesResponseType(StatusCodes.Status204NoContent)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] TCommand command, CancellationToken ct)
//        => HandleResult(await Mediator.Send(BuildUpdateCommand(id, command), ct));

//    /// <summary>
//    /// Delete entity
//    /// </summary>
//    [HttpDelete("{id:guid}")]
//    [ProducesResponseType(StatusCodes.Status204NoContent)]
//    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
//        => HandleResult(await Mediator.Send(BuildDeleteCommand(id), ct));

//    // Override these in derived controllers to build specific commands/queries
//    protected abstract IQuery BuildGetAllQuery(PaginatedRequest filter);
//    protected abstract IQuery BuildGetByIdQuery(Guid id);
//    protected abstract ICommand BuildCreateCommand(TCommand command);
//    protected abstract ICommand BuildUpdateCommand(Guid id, TCommand command);
//    protected abstract ICommand BuildDeleteCommand(Guid id);
//}
