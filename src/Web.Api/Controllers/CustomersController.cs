using Application.Abstractions.Messaging;
using Application.Customers.Create;
using Application.Customers.Delete;
using Application.Customers.DTOs;
using Application.Customers.Get;
using Application.Customers.GetById;
using Application.Customers.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Web.Api.Controllers;

/// <summary>
/// Customers API endpoints for managing customer resources.
/// Provides full CRUD operations with proper validation and error handling.
/// </summary>
[Route("api/customers")]
[ApiController]
[Authorize]
[Tags("Customers")]
public class CustomersController(
    ICommandHandler<CreateCustomerCommand, Guid> createCommandHandler,
    IQueryHandler<GetCustomersQuery, List<CustomerDto>> getCustomersQueryHandler,
    IQueryHandler<GetCustomerByIdQuery, CustomerDto> getCustomerByIdQueryHandler,
    ICommandHandler<UpdateCustomerCommand> updateCommandHandler,
    ICommandHandler<DeleteCustomerCommand> deleteCommandHandler) : ApiController
{
    /// <summary>
    /// Retrieves all customers.
    /// </summary>
    /// <response code="200">Customers retrieved successfully.</response>
    /// <response code="400">Bad request.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCustomers(CancellationToken cancellationToken)
    {
        var result = await getCustomersQueryHandler.Handle(new GetCustomersQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a specific customer by ID.
    /// </summary>
    /// <param name="id">The customer's unique identifier.</param>
    /// <response code="200">Customer retrieved successfully.</response>
    /// <response code="404">Customer not found.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await getCustomerByIdQueryHandler.Handle(new GetCustomerByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="request">The customer creation request.</param>
    /// <response code="201">Customer created successfully.</response>
    /// <response code="400">Bad request or validation error.</response>
    /// <response code="409">Email already exists.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
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
    /// Updates an existing customer.
    /// </summary>
    /// <param name="id">The customer's unique identifier.</param>
    /// <param name="request">The customer update request.</param>
    /// <response code="204">Customer updated successfully.</response>
    /// <response code="400">Bad request or validation error.</response>
    /// <response code="404">Customer not found.</response>
    /// <response code="409">Email already exists.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.Address);

        var result = await updateCommandHandler.Handle(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Deletes a customer by ID.
    /// </summary>
    /// <param name="id">The customer's unique identifier.</param>
    /// <response code="204">Customer deleted successfully.</response>
    /// <response code="404">Customer not found.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await deleteCommandHandler.Handle(new DeleteCustomerCommand(id), cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// Request model for creating a new customer.
/// </summary>
public sealed class CreateCustomerRequest
{
    /// <summary>Customer's full name (required).</summary>
    public required string Name { get; set; }

    /// <summary>Customer's email address (required).</summary>
    public required string Email { get; set; }

    /// <summary>Customer's phone number (optional).</summary>
    public string? Phone { get; set; }

    /// <summary>Customer's address (optional).</summary>
    public string? Address { get; set; }
}

/// <summary>
/// Request model for updating an existing customer.
/// </summary>
public sealed class UpdateCustomerRequest
{
    /// <summary>Customer's full name (required).</summary>
    public required string Name { get; set; }

    /// <summary>Customer's email address (required).</summary>
    public required string Email { get; set; }

    /// <summary>Customer's phone number (optional).</summary>
    public string? Phone { get; set; }

    /// <summary>Customer's address (optional).</summary>
    public string? Address { get; set; }
}
