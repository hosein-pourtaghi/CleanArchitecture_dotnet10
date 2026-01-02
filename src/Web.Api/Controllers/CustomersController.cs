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
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Controllers;

[ApiController]
[Route("customers")]
[Authorize]
[Tags("Customers")]
public class CustomersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        IQueryHandler<GetCustomersQuery, List<CustomerDto>> handler,
        CancellationToken cancellationToken)
    {
        Result<List<CustomerDto>> result = await handler.Handle(new GetCustomersQuery(), cancellationToken);
        return result.Match(Ok, CustomResults.Problem);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomerById(
        Guid id,
        IQueryHandler<GetCustomerByIdQuery, CustomerDto> handler,
        CancellationToken cancellationToken)
    {
        Result<CustomerDto> result = await handler.Handle(new GetCustomerByIdQuery(id), cancellationToken);
        return result.Match(Ok, CustomResults.Problem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(
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

        Result<Guid> result = await handler.Handle(command, cancellationToken);
        return result.Match(Ok, CustomResults.Problem);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(
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

        Result result = await handler.Handle(command, cancellationToken);
        return result.Match(_ => NoContent(), CustomResults.Problem);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(
        Guid id,
        ICommandHandler<DeleteCustomerCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeleteCustomerCommand(id), cancellationToken);
        return result.Match(_ => NoContent(), CustomResults.Problem);
    }
}

public sealed class CreateCustomerRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public sealed class UpdateCustomerRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}
