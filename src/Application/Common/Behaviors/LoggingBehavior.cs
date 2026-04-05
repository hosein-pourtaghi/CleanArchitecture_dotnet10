using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for logging request handling.
/// Logs the execution time and details of each request processed.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
internal sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var traceId = Activity.Current?.Id ?? "no-trace-id";

        _logger.LogInformation(
            "Handling {RequestType}. TraceId: {TraceId}",
            requestType,
            traceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "Successfully handled {RequestType}. Duration: {ElapsedMilliseconds}ms. TraceId: {TraceId}",
                requestType,
                stopwatch.ElapsedMilliseconds,
                traceId);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            _logger.LogError(
                exception,
                "Error handling {RequestType}. Duration: {ElapsedMilliseconds}ms. TraceId: {TraceId}",
                requestType,
                stopwatch.ElapsedMilliseconds,
                traceId);

            throw;
        }
    }
}
