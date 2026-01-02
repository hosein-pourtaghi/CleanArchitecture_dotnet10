using Application.Abstractions.Messaging;
using Application.Customers.Get;
using Application.Customers.DTOs;
using SharedKernel;
using Web.Api.Endpoints;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Customers;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("customers", async (
            IQueryHandler<GetCustomersQuery, List<CustomerDto>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<CustomerDto>> result = await handler.Handle(new GetCustomersQuery(), cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Customers)
        .RequireAuthorization();
    }
}