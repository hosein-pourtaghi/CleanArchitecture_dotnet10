using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DynamicCrud;
using Application.Common.DynamicCrud;


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



    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {

        var result =
            await _mediator.Send(
                new DynamicGetAllQuery<TEntity>(),
                cancellationToken);


        return HandleResult(result);
    }



    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {

        var result =
            await _mediator.Send(
                new DynamicGetByIdQuery<TEntity>(id),
                cancellationToken);


        return HandleResult(result);
    }



    [HttpPost]
    public async Task<IActionResult> Create(
        TEntity entity,
        CancellationToken cancellationToken)
    {

        var result =
            await _mediator.Send(
                new DynamicCreateCommand<TEntity>(entity),
                cancellationToken);


        return HandleCreatedResult(
            result,
            x => x.Id);
    }



    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        TEntity entity,
        CancellationToken cancellationToken)
    {

        var result =
            await _mediator.Send(
                new DynamicUpdateCommand<TEntity>(
                    id,
                    entity),
                cancellationToken);


        return HandleResult(result);
    }



    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {

        var result =
            await _mediator.Send(
                new DynamicDeleteCommand<TEntity>(id),
                cancellationToken);


        return HandleResult(result);
    }
}
