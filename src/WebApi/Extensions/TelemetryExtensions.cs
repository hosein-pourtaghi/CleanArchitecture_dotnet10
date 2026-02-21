using System.Diagnostics;
using WebApi.Telemetry;
using Microsoft.Extensions.Logging;

namespace WebApi.Extensions;

public static class TelemetryExtensions
{
    /// ✅ Uses standard Activity.Current.Id (no parameters needed)
    public static Activity? StartOperationSpan(this ILogger logger, string operationName)
    {
        var activity = TelemetryActivitySource.Instance.StartActivity(operationName);
        logger.LogDebug("Started operation: {OperationName}, TraceId: {TraceId}",
            operationName, activity?.Id);
        return activity;
    }

    /// ✅ Uses standard Activity.Current.Id (no correlationId parameter)
    public static void LogOperationError(
        this ILogger logger,
        Exception exception,
        string operationName)
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
            "Operation failed: {OperationName}, TraceId: {TraceId}",
            operationName,
            activity?.Id ?? "N/A");
    }

    /// ✅ Uses standard Activity.Current.Id
    public static void LogOperationSuccess(
        this ILogger logger,
        string operationName,
        long elapsedMs)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("operation.duration_ms", elapsedMs);
        }
        logger.LogInformation(
            "Operation completed successfully: {OperationName}, DurationMs: {ElapsedMs}, TraceId: {TraceId}",
            operationName,
            elapsedMs,
            activity?.Id ?? "N/A");
    }
}
