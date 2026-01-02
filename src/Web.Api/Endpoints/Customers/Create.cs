using Application.Abstractions.Messaging;
using Application.Customers.Create;
using SharedKernel;
using Web.Api.Endpoints;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Customers;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("customers", async (
            Request request,
            ICommandHandler<CreateCustomerCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCustomerCommand
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address
            };

            Result<Guid> result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Customers)
        .RequireAuthorization();
    }
}