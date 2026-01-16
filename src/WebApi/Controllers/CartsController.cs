using Application.Abstractions.Messaging;
using Application.Carts.Copy;
using Application.Carts.Create;
using Application.Carts.Get;
using Application.Carts.GetById;
using Application.Carts.Update;
using Application.Common.DTOs;
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
public class CartsController(
    ICommandHandler<CreateCartCommand, Guid> createCommandHandler,
    ICommandHandler<CopyCartCommand, bool> copyCommandHandler,
    IQueryHandler<GetCartsQuery, List<CartDto>> getCartsQueryHandler,
    IQueryHandler<GetCartByIdQuery, CartDto> getCartByIdQueryHandler,
    ICommandHandler<UpdateCartCommand> updateCommandHandler
    //,
    //ICommandHandler<DeleteCartCommand> deleteCommandHandler
    ) : ApiController
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    public async Task<IActionResult> GetCarts(CancellationToken cancellationToken)
    {
        var result = await getCartsQueryHandler.Handle(new GetCartsQuery(), cancellationToken);
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await getCartByIdQueryHandler.Handle(new GetCartByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new cart.
    /// </summary>
    /// <remarks>
    /// Creates a new cart record with the provided information.
    /// 
    /// ## Request Body
    /// ```json
    /// {
    ///   "name": "John Doe",
    ///   "email": "john@example.com",
    ///   "phone": "+1-555-0123",
    ///   "address": "123 Main St, Anytown, ST 12345"
    /// }
    /// ```
    /// 
    /// ## Validation Rules
    /// - **Name**: Required, 1-200 characters
    /// - **Email**: Required, valid email format, max 255 characters, must be unique
    /// - **Phone**: Optional, max 20 characters
    /// - **Address**: Optional, max 500 characters
    /// 
    /// ## Response
    /// Returns HTTP 201 (Created) with the new cart ID and location header.
    /// 
    /// ## Domain Events
    /// Triggers:
    /// - `CartCreatedDomainEvent` - Published to message bus
    /// - Audit log entry created for compliance
    /// 
    /// ## Error Cases
    /// - 400: Validation error (invalid email, missing required fields)
    /// - 409: Email already exists
    /// - 401: Unauthorized
    /// </remarks>
    /// <param name="request">Cart creation request</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Created cart ID</returns>
    /// <response code="201">Cart created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="409">Email already exists</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCartRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCartCommand(
            request.Name,
            request.Email,
            request.Phone,
            request.Address);

        var result = await createCommandHandler.Handle(command, cancellationToken);

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
    /// Updates cart information. All fields are required.
    /// 
    /// ## Request Body
    /// ```json
    /// {
    ///   "name": "Jane Doe",
    ///   "email": "jane@example.com",
    ///   "phone": "+1-555-0456",
    ///   "address": "456 Oak Ave, Somewhere, ST 54321"
    /// }
    /// ```
    /// 
    /// ## Validation Rules
    /// Same as creation endpoint - see POST endpoint documentation.
    /// 
    /// ## Response
    /// Returns HTTP 204 (No Content) on success.
    /// 
    /// ## Domain Events
    /// Triggers:
    /// - `CartUpdatedDomainEvent` - Published to message bus
    /// - Audit log entry created
    /// 
    /// ## Error Handling
    /// - 404: Cart not found
    /// - 409: Email already in use by another cart
    /// - 400: Validation error
    /// </remarks>
    /// <param name="id">The cart's unique identifier</param>
    /// <param name="request">Update request with new cart data</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Cart updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Cart not found</response>
    /// <response code="409">Email already in use</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCartRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCartCommand(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.Address);

        var result = await updateCommandHandler.Handle(command, cancellationToken);
        return HandleResult(result);
    }

    ///// <summary>
    ///// Deletes a cart.
    ///// </summary>
    ///// <remarks>
    ///// Permanently removes a cart record from the system.
    ///// 
    ///// ## Security Considerations
    ///// - This operation is permanent and cannot be undone
    ///// - Audit log records are retained for compliance
    ///// - Consider implementing soft delete if data retention is required
    ///// 
    ///// ## Domain Events
    ///// Triggers:
    ///// - `CartDeletedDomainEvent` - Published to message bus
    ///// - Audit log entry created
    ///// 
    ///// ## Cascading Effects
    ///// - Associated orders may be affected (depends on database constraints)
    ///// - Notification services will be triggered
    ///// 
    ///// ## Error Handling
    ///// - 404: Cart not found
    ///// </remarks>
    ///// <param name="id">The cart's unique identifier</param>
    ///// <param name="cancellationToken">Cancellation token for the operation</param>
    ///// <returns>No content on success</returns>
    ///// <response code="204">Cart deleted successfully</response>
    ///// <response code="404">Cart not found</response>
    ///// <response code="401">Unauthorized - invalid or missing token</response>
    //[HttpDelete("{id:guid}")]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //public async Task<IActionResult> Delete(
    //    [FromRoute] Guid id,
    //    CancellationToken cancellationToken)
    //{
    //    var result = await deleteCommandHandler.Handle(new DeleteCartCommand(id), cancellationToken);
    //    return HandleResult(result);
    //}


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Copy( 
          CancellationToken cancellationToken)
    {
        var command = new CopyCartCommand( );

        var result = await copyCommandHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleResult<bool>(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
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
    /// <summary>Cart's full name (required). Must be 1-200 characters.</summary>
    /// <example>John Doe</example>
    public required string Name { get; set; }

    /// <summary>Cart's email address (required). Must be unique and valid email format.</summary>
    /// <example>john.doe@example.com</example>
    public required string Email { get; set; }

    /// <summary>Cart's phone number (optional). Maximum 20 characters.</summary>
    /// <example>+1-555-0123</example>
    public string? Phone { get; set; }

    /// <summary>Cart's physical address (optional). Maximum 500 characters.</summary>
    /// <example>123 Main Street, Anytown, ST 12345</example>
    public string? Address { get; set; }
}
 

/// <summary>
/// Request model for updating an existing cart.
/// </summary>
/// <remarks>
/// All fields are required when updating. To keep a field unchanged, provide its current value.
/// </remarks>
public sealed class UpdateCartRequest
{
    /// <summary>Cart's full name (required). Must be 1-200 characters.</summary>
    /// <example>Jane Doe</example>
    public required string Name { get; set; }

    /// <summary>Cart's email address (required). Must be valid email format.</summary>
    /// <example>jane.doe@example.com</example>
    public required string Email { get; set; }

    /// <summary>Cart's phone number (optional). Maximum 20 characters.</summary>
    /// <example>+1-555-0456</example>
    public string? Phone { get; set; }

    /// <summary>Cart's physical address (optional). Maximum 500 characters.</summary>
    /// <example>456 Oak Avenue, Somewhere, ST 54321</example>
    public string? Address { get; set; }
}
