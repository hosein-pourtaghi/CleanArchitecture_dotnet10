using SharedKernel;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Handles a query that reads data without modifying state.
/// The query must implement IQuery&lt;TResponse&gt;.
/// </summary>
/// <typeparam name="TQuery">The query type implementing IQuery&lt;TResponse&gt;</typeparam>
/// <typeparam name="TResponse">The response type returned by the query</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handles the query execution and returns a response.
    /// </summary>
    /// <param name="query">The query to handle</param>
    /// <param name="cancellationToken">Cancellation token for long-running operations</param>
    /// <returns>A result containing the response or an error</returns>
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
