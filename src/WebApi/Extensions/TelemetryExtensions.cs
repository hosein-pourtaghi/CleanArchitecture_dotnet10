using System.Diagnostics;
using WebApi.Telemetry;

namespace WebApi.Extensions;

/// <summary>
/// Extension methods for OpenTelemetry integration in the Web API.
/// Provides convenience methods for creating spans and logging traces.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Starts a span for a specific operation with automatic error handling and timing.
    /// </summary>
    public static Activity? StartOperationSpan(this ILogger logger, string operationName, string? correlationId = null)
    {
        var activity = TelemetryActivitySource.Instance.StartActivity(operationName);

        if (activity != null && !string.IsNullOrEmpty(correlationId))
        {
            activity.SetTag("correlation.id", correlationId);
        }

        logger.LogDebug("Started operation: {OperationName}, TraceId: {TraceId}", operationName, activity?.Id);

        return activity;
    }

    /// <summary>
    /// Logs an operation error with trace context.
    /// </summary>
    public static void LogOperationError(
        this ILogger logger,
        Exception exception,
        string operationName,
        string? correlationId = null)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("exception.type", exception.GetType().Name);
            activity.SetTag("exception.message", exception.Message);
            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        }

        logger.LogError(
            exception,
            "Operation failed: {OperationName}, CorrelationId: {CorrelationId}, TraceId: {TraceId}",
            operationName,
            correlationId ?? "N/A",
            activity?.Id ?? "N/A");
    }

    /// <summary>
    /// Logs an operation success with timing information.
    /// </summary>
    public static void LogOperationSuccess(
        this ILogger logger,
        string operationName,
        long elapsedMs,
        string? correlationId = null)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("operation.duration_ms", elapsedMs);
        }

        logger.LogInformation(
            "Operation completed successfully: {OperationName}, DurationMs: {ElapsedMs}, CorrelationId: {CorrelationId}, TraceId: {TraceId}",
            operationName,
            elapsedMs,
            correlationId ?? "N/A",
            activity?.Id ?? "N/A");
    }
}
