using MediatR;
using SharedKernel;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Handles a query that reads data without modifying state.
/// The query must implement IQuery&lt;TResponse&gt;.
/// Implements MediatR IRequestHandler for pipeline behavior support.
/// </summary>
/// <typeparam name="TQuery">The query type implementing IQuery&lt;TResponse&gt;</typeparam>
/// <typeparam name="TResponse">The response type returned by the query</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
