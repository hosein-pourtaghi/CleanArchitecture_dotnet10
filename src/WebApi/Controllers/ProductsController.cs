using Application.Abstractions.Messaging;
using Application.Common.DTOs; 
using Application.Products.Create;
using Application.Products.Delete;
using Application.Products.GetAll;
using Application.Products.GetById;
using Application.Products.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 

namespace WebApi.Controllers;
 
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize] 
public class ProductsController(
    ICommandHandler<CreateProductCommand, Guid> createCommandHandler, 
    IQueryHandler<GetAllProductQuery, List<ProductDto>> getAllProductQueryHandler,
    IQueryHandler<GetByIdProductQuery, ProductDto> getByIdProductQueryHandler,
    ICommandHandler<UpdateProductCommand> updateCommandHandler,
    ICommandHandler<DeleteProductCommand> deleteCommandHandler) : ApiController
{
    
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    public async Task<IActionResult> GetAllProduct(CancellationToken cancellationToken)
    {
        var result = await getAllProductQueryHandler.Handle(new GetAllProductQuery(), cancellationToken);
        return HandleResult(result);
    }
 
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await getByIdProductQueryHandler.Handle(new GetByIdProductQuery(id), cancellationToken);
        return HandleResult(result);
    }
 
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            
        );

        var result = await createCommandHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleResult<Guid>(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            request.Id
        );

        var result = await updateCommandHandler.Handle(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await deleteCommandHandler.Handle(new DeleteProductCommand(id), cancellationToken);
        return HandleResult(result);
    } 

}

  
public sealed class CreateProductRequest
{ 

}
 
public sealed class UpdateProductRequest
{
    public required Guid Id { get; set; }

}

