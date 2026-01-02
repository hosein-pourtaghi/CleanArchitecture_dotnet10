using Application.Abstractions.Messaging;
using Application.Customers.Delete;
using SharedKernel;
using Web.Api.Endpoints;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Customers;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("customers/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteCustomerCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new DeleteCustomerCommand(id), cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Customers)
        .RequireAuthorization();
    }
}