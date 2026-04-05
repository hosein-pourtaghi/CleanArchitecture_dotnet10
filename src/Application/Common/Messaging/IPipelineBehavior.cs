using SharedKernel;

namespace Application.Common.Messaging;

/// <summary>
/// Represents a pipeline behavior that can intercept request handling.
/// Useful for implementing cross-cutting concerns like logging, validation, caching, etc.
/// Similar to middleware in the request pipeline.
/// </summary>
/// <typeparam name="TRequest">The request type (command or query) implementing IRequest&lt;TResponse&gt;</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request and can invoke the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token for long-running operations</param>
    /// <returns>The result from the pipeline</returns>
    Task<Result<TResponse>> Handle(
        TRequest request,
        Func<Task<Result<TResponse>>> next,
        CancellationToken cancellationToken);
}
