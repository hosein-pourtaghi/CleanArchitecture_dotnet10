using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Telemetry;

namespace Web.Api.Infrastructure;

/// <summary>
/// Global exception handler with OpenTelemetry instrumentation.
/// Logs exceptions and creates trace events for better observability.
/// </summary>
internal sealed class OpenTelemetryExceptionHandler(ILogger<OpenTelemetryExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        using (var activity = TelemetryActivitySource.Instance.StartActivity("HandleException"))
        {
            activity?.SetTag("exception.type", exception.GetType().Name);
            activity?.SetTag("exception.message", exception.Message);
            activity?.SetTag("exception.stacktrace", exception.StackTrace);
            activity?.SetTag("http.status_code", StatusCodes.Status500InternalServerError);
            activity?.SetTag("http.method", httpContext.Request.Method);
            activity?.SetTag("http.url", httpContext.Request.Path);

            // Extract correlation ID from context
            var correlationId = httpContext.TraceIdentifier;
            activity?.SetTag("correlation.id", correlationId);

            logger.LogError(
                exception,
                "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
                correlationId,
                httpContext.Request.Path,
                httpContext.Request.Method);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                Title = "Server failure",
                Extensions = new Dictionary<string, object?>
                {
                    { "correlation-id", correlationId }
                }
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }
}
