using Application.Carts.Create;
using Application.Carts.Generate;
using Application.Carts.Get;
using Application.Carts.GetById;
using Application.Carts.GetCustomerCarts;
using Application.Carts.Update;
using Application.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

/// <summary>
/// Carts API endpoints for managing cart resources.
/// Provides full CRUD operations with comprehensive documentation and error handling.
/// All endpoints require JWT authentication.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
[Tags("Carts")]
[Produces("application/json")]
[Consumes("application/json")]
public class CartsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Retrieves all carts.
    /// </summary>
    /// <remarks>
    /// Fetches a complete list of all carts in the system.
    /// 
    /// ## Response
    /// Returns an array of cart objects sorted by creation date (newest first).
    /// 
    /// ## Security
    /// Requires valid JWT token in Authorization header.
    /// 
    /// ## Performance
    /// Results are cached for 5 minutes. For real-time data, use the cart-by-id endpoint.
    /// </remarks>
    /// <returns>List of all carts</returns>
    /// <response code="200">Carts retrieved successfully</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CartDto>), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> GetAllCart(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllCartQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a specific cart by ID.
    /// </summary>
    /// <remarks>
    /// Fetches detailed information for a single cart.
    /// 
    /// ## Parameters
    /// - **id**: The unique identifier (GUID) of the cart to retrieve
    /// 
    /// ## Response
    /// Returns complete cart details including contact information and timestamps.
    /// 
    /// ## Error Handling
    /// Returns 404 if the cart ID does not exist.
    /// 
    /// ## Security
    /// Requires valid JWT token. Users can view any cart (implement role-based filtering if needed).
    /// </remarks>
    /// <param name="id">The cart's unique identifier (GUID)</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Cart details</returns>
    /// <response code="200">Cart retrieved successfully</response>
    /// <response code="404">Cart not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCartByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves all carts for a specific customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(List<CartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerCarts(
        [FromRoute] Guid customerId,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCustomerCartsQuery(customerId, page, pageSize), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new cart.
    /// </summary>
    /// <remarks>
    /// Creates a new cart record with the provided information.
    /// </remarks>
    /// <param name="request">Cart creation request</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Created cart ID</returns>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCartRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCartCommand(
            request.CustomerId,
            request.Currency,
            request.TransactionId,
            request.PaymentType,
            request.Code,
            request.PurchaseDate,
            request.CartItems ?? new());
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleResult<Guid>(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>
    /// Updates an existing cart.
    /// </summary>
    /// <remarks>
    /// Updates cart information.
    /// </remarks>
    /// <param name="id">The cart's unique identifier</param>
    /// <param name="request">Update request with new cart data</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCartRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCartCommand(
            request.Id,
            request.CustomerId,
            request.Currency,
            request.TransactionId,
            request.PaymentType,
            request.Code,
            request.PurchaseDate,
            request.CartItems ?? new());
        var result = await mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Generates sample carts.
    /// </summary>
    /// <remarks>
    /// Creates sample cart data for testing.
    /// </remarks>
    [HttpPost("generate")]
    [AllowAnonymous]
    public async Task<IActionResult> Generate(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GenerateCartCommand(), cancellationToken);
        return HandleResult(result);
    }
}




/// <summary>
/// Request model for creating a new cart.
/// </summary>
/// <remarks>
/// All fields follow clean data requirements:
/// - No HTML/script injection
/// - SQL injection prevention
/// - Proper encoding for special characters
/// </remarks>
public sealed class CreateCartRequest
{
    public Guid CustomerId { get; set; }
    public string Currency { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentType { get; set; }
    public string? Code { get; set; }
    public DateTime PurchaseDate { get; set; }
    public List<CartItemDto> CartItems { get; set; }
}


/// <summary>
/// Request model for updating an existing cart.
/// </summary>
/// <remarks>
/// All fields are required when updating. To keep a field unchanged, provide its current value.
/// </remarks>
public sealed class UpdateCartRequest
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Currency { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentType { get; set; }
    public string? Code { get; set; }
    public DateTime PurchaseDate { get; set; }
    public List<CartItemDto> CartItems { get; set; }
}
