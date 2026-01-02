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
/// </summary>
[Route("customers")]
[Authorize]
[Tags("Customers")]
public class CustomersController : ApiController
{
    /// <summary>
    /// Retrieves all customers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCustomers(
        IQueryHandler<GetCustomersQuery, List<CustomerDto>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetCustomersQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a specific customer by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        Guid id,
        IQueryHandler<GetCustomerByIdQuery, CustomerDto> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetCustomerByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        CreateCustomerRequest request,
        ICommandHandler<CreateCustomerCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address
        };

        var result = await handler.Handle(command, cancellationToken);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Value });
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCustomerRequest request,
        ICommandHandler<UpdateCustomerCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand
        {
            Id = id,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address
        };

        var result = await handler.Handle(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Deletes a customer by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        Guid id,
        ICommandHandler<DeleteCustomerCommand> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new DeleteCustomerCommand(id), cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// Request model for creating a new customer.
/// </summary>
public sealed class CreateCustomerRequest
{
    /// <summary>Customer name.</summary>
    public string Name { get; set; }

    /// <summary>Customer email address.</summary>
    public string Email { get; set; }

    /// <summary>Optional customer phone number.</summary>
    public string? Phone { get; set; }

    /// <summary>Optional customer address.</summary>
    public string? Address { get; set; }
}

/// <summary>
/// Request model for updating an existing customer.
/// </summary>
public sealed class UpdateCustomerRequest
{
    /// <summary>Customer name.</summary>
    public string Name { get; set; }

    /// <summary>Customer email address.</summary>
    public string Email { get; set; }

    /// <summary>Optional customer phone number.</summary>
    public string? Phone { get; set; }

    /// <summary>Optional customer address.</summary>
    public string? Address { get; set; }
}
