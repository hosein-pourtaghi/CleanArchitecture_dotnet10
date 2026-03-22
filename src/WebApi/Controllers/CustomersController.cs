using Application.Common.DTOs;
using Application.Customers.Copy;
using Application.Customers.Create;
using Application.Customers.Delete;
using Application.Customers.Get;
using Application.Customers.GetById;
using Application.Customers.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

/// <summary>
/// Customers API endpoints for managing customer resources.
/// Provides full CRUD operations with comprehensive documentation and error handling.
/// All endpoints require JWT authentication.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
[Tags("Customers")]
[Produces("application/json")]
[Consumes("application/json")]
public class CustomersController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Retrieves all customers.
    /// </summary>
    /// <remarks>
    /// Fetches a complete list of all customers in the system.
    /// 
    /// ## Response
    /// Returns an array of customer objects sorted by creation date (newest first).
    /// 
    /// ## Security
    /// Requires valid JWT token in Authorization header.
    /// 
    /// ## Performance
    /// Results are cached for 5 minutes. For real-time data, use the customer-by-id endpoint.
    /// </remarks>
    /// <returns>List of all customers</returns>
    /// <response code="200">Customers retrieved successfully</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> GetCustomers(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCustomersQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a specific customer by ID.
    /// </summary>
    /// <remarks>
    /// Fetches detailed information for a single customer.
    /// 
    /// ## Parameters
    /// - **id**: The unique identifier (GUID) of the customer to retrieve
    /// 
    /// ## Response
    /// Returns complete customer details including contact information and timestamps.
    /// 
    /// ## Error Handling
    /// Returns 404 if the customer ID does not exist.
    /// 
    /// ## Security
    /// Requires valid JWT token. Users can view any customer (implement role-based filtering if needed).
    /// </remarks>
    /// <param name="id">The customer's unique identifier (GUID)</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Customer details</returns>
    /// <response code="200">Customer retrieved successfully</response>
    /// <response code="404">Customer not found</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <remarks>
    /// Creates a new customer record with the provided information.
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
    /// Returns HTTP 201 (Created) with the new customer ID and location header.
    /// 
    /// ## Domain Events
    /// Triggers:
    /// - `CustomerCreatedDomainEvent` - Published to message bus
    /// - Audit log entry created for compliance
    /// 
    /// ## Error Cases
    /// - 400: Validation error (invalid email, missing required fields)
    /// - 409: Email already exists
    /// - 401: Unauthorized
    /// </remarks>
    /// <param name="request">Customer creation request</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Created customer ID</returns>
    /// <response code="201">Customer created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="409">Email already exists</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.Name,
            request.Email,
            request.Phone,
            request.Address);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleResult<Guid>(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <remarks>
    /// Updates customer information. All fields are required.
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
    /// - `CustomerUpdatedDomainEvent` - Published to message bus
    /// - Audit log entry created
    /// 
    /// ## Error Handling
    /// - 404: Customer not found
    /// - 409: Email already in use by another customer
    /// - 400: Validation error
    /// </remarks>
    /// <param name="id">The customer's unique identifier</param>
    /// <param name="request">Update request with new customer data</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Customer updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Customer not found</response>
    /// <response code="409">Email already in use</response>
    /// <response code="401">Unauthorized - invalid or missing token</response>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.Address);

        var result = await mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Deletes a customer.
    /// </summary>
    /// <remarks>
    /// Permanently removes a customer record from the system.
    /// 
    /// ## Security Considerations
    /// - This operation is permanent and cannot be undone
    /// - Audit log records are retained for compliance
    /// - Consider implementing soft delete if data retention is required
    /// 
    /// ## Domain Events
    /// Triggers:
    /// - `CustomerDeletedDomainEvent` - Published to message bus
    /// - Audit log entry created
    /// 
    /// ## Cascading Effects
    /// - Associated carts may be affected (depends on database constraints)
    /// - Notification services will be triggered
    /// 
    /// ## Error Handling
    /// - 404: Customer not found
    /// </remarks>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCustomerCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Copies an existing customer.
    /// </summary>
    /// <remarks>
    /// Creates a copy of an existing customer with all the same details.
    /// </remarks>
    /// <param name="id">The customer ID to copy</param>
    /// <param name="request">The customer data to copy</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/copy")]
    public async Task<IActionResult> Copy(
        [FromRoute] Guid id,
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CopyCustomerCommand(request.Name, request.Email, request.Phone, request.Address);
        var result = await mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}


/// <summary>
/// Request model for creating a new customer.
/// </summary>
/// <remarks>
/// All fields follow clean data requirements:
/// - No HTML/script injection
/// - SQL injection prevention
/// - Proper encoding for special characters
/// </remarks>
public sealed class CreateCustomerRequest
{
    /// <summary>Customer's full name (required). Must be 1-200 characters.</summary>
    /// <example>John Doe</example>
    public required string Name { get; set; }

    /// <summary>Customer's email address (required). Must be unique and valid email format.</summary>
    /// <example>john.doe@example.com</example>
    public required string Email { get; set; }

    /// <summary>Customer's phone number (optional). Maximum 20 characters.</summary>
    /// <example>+1-555-0123</example>
    public string? Phone { get; set; }

    /// <summary>Customer's physical address (optional). Maximum 500 characters.</summary>
    /// <example>123 Main Street, Anytown, ST 12345</example>
    public string? Address { get; set; }
}

public sealed class CopyCustomerRequest
{
    /// <summary>Customer's full name (required). Must be 1-200 characters.</summary>
    /// <example>John Doe</example>
    public required string Name { get; set; }

    /// <summary>Customer's email address (required). Must be unique and valid email format.</summary>
    /// <example>john.doe@example.com</example>
    public required string Email { get; set; }

    /// <summary>Customer's phone number (optional). Maximum 20 characters.</summary>
    /// <example>+1-555-0123</example>
    public string? Phone { get; set; }

    /// <summary>Customer's physical address (optional). Maximum 500 characters.</summary>
    /// <example>123 Main Street, Anytown, ST 12345</example>
    public string? Address { get; set; }
}

/// <summary>
/// Request model for updating an existing customer.
/// </summary>
/// <remarks>
/// All fields are required when updating. To keep a field unchanged, provide its current value.
/// </remarks>
public sealed class UpdateCustomerRequest
{
    /// <summary>Customer's full name (required). Must be 1-200 characters.</summary>
    /// <example>Jane Doe</example>
    public required string Name { get; set; }

    /// <summary>Customer's email address (required). Must be valid email format.</summary>
    /// <example>jane.doe@example.com</example>
    public required string Email { get; set; }

    /// <summary>Customer's phone number (optional). Maximum 20 characters.</summary>
    /// <example>+1-555-0456</example>
    public string? Phone { get; set; }

    /// <summary>Customer's physical address (optional). Maximum 500 characters.</summary>
    /// <example>456 Oak Avenue, Somewhere, ST 54321</example>
    public string? Address { get; set; }
}
