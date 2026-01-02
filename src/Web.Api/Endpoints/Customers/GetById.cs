using Application.Abstractions.Messaging;
using Application.Customers.DTOs;
using Application.Customers.GetById;
using SharedKernel;
using Web.Api.Endpoints;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Customers;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("customers/{id:guid}", async (
            Guid id,
            IQueryHandler<GetCustomerByIdQuery, CustomerDto> handler,
            CancellationToken cancellationToken) =>
        {
            Result<CustomerDto> result = await handler.Handle(new GetCustomerByIdQuery(id), cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Customers)
        .RequireAuthorization();
    }
}