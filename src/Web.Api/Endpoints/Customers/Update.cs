using Application.Abstractions.Messaging;
using Application.Customers.Update;
using SharedKernel;
using Web.Api.Endpoints;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Customers;

internal sealed class Update : IEndpoint
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
        app.MapPut("customers/{id:guid}", async (
            Guid id,
            Request request,
            ICommandHandler<UpdateCustomerCommand> handler,
            CancellationToken cancellationToken) =>
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
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Customers)
        .RequireAuthorization();
    }
}