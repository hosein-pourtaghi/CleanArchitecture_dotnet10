using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DynamicCrud;
using Application.Common.DynamicCrud.Commands;
using Application.Common.DynamicCrud.Queries;
namespace SharedApi.Controllers;


[Route("[controller]")]
public abstract class DynamicCrudController<TEntity>
    : ApiController
    where TEntity : class, IDynamicCrudEntity
{

    private readonly IMediator _mediator;


    protected DynamicCrudController(
        IMediator mediator)
    {
        _mediator = mediator;
    }



    // ============================================================
    // GET ALL
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {

        var result =
            await _mediator.Send(
                new GetDynamicCrudListQuery<TEntity>(),
                cancellationToken);


        return HandleResult(result);
    }



    // ============================================================
    // GET BY ID
    // ============================================================

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {

        var result =
            await _mediator.Send(
                new GetDynamicCrudByIdQuery<TEntity>(id),
                cancellationToken);


        return HandleResult(result);
    }



    // ============================================================
    // CREATE
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> Create(
        TEntity entity,
        CancellationToken cancellationToken)
    {

        var result =
            await _mediator.Send(
                new CreateDynamicCrudCommand<TEntity>(
                    entity),
                cancellationToken);


        return HandleCreatedResult(
            result,
            x => x.Id);
    }



    // ============================================================
    // UPDATE
    // ============================================================

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        TEntity entity,
        CancellationToken cancellationToken)
    {

        var result =
            await _mediator.Send(
                new UpdateDynamicCrudCommand<TEntity>(
                    id,
                    entity),
                cancellationToken);


        return HandleResult(result);
    }



    // ============================================================
    // DELETE
    // ============================================================

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {

        var result =
            await _mediator.Send(
                new DeleteDynamicCrudCommand<TEntity>(
                    id),
                cancellationToken);


        return HandleResult(result);
    }
}
