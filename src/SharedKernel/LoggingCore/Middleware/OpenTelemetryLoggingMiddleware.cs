// src/LoggingLibrary/Middleware/OpenTelemetryLoggingMiddleware.cs
// ============================================================
// PURPOSE: Middleware for OpenTelemetry request/response logging
// ============================================================

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedKernel.LoggingCore.Telemetry;

namespace LoggingLibrary.Middleware;

/// <summary>
/// Middleware for creating spans and logging HTTP request/response details with OpenTelemetry.
/// Captures timing information, status codes, and errors for better observability.
/// </summary>
public class OpenTelemetryLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<OpenTelemetryLoggingMiddleware> _logger;
    private const string CorrelationIdHeaderName = "Correlation-Id";

    public OpenTelemetryLoggingMiddleware(
        RequestDelegate next,
        ILogger<OpenTelemetryLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract or create correlation ID
        var correlationId = GetOrCreateCorrelationId(context);

        // Create an activity for the HTTP request
        using (var activity = TelemetryActivitySource.Instance.StartActivity(
            $"{context.Request.Method} {context.Request.Path}"))
        {
            activity?.SetTag("http.method", context.Request.Method);
            activity?.SetTag("http.url", context.Request.Path);
            activity?.SetTag("http.scheme", context.Request.Scheme);
            activity?.SetTag("http.host", context.Request.Host);
            activity?.SetTag("http.user_agent", context.Request.Headers.UserAgent.ToString());
            activity?.SetTag("correlation.id", correlationId);

            // Capture the start time
            var startTime = Stopwatch.GetTimestamp();

            try
            {
                _logger.LogInformation(
                    "HTTP request started. Method: {Method}, Path: {Path}, CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId);

                await _next.Invoke(context);

                // Calculate elapsed time
                var elapsedMs = GetElapsedMilliseconds(startTime);

                activity?.SetTag("http.status_code", context.Response.StatusCode);
                activity?.SetTag("http.response_time_ms", elapsedMs);

                _logger.LogInformation(
                    "HTTP request completed. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, ElapsedMs: {ElapsedMs}, CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsedMs,
                    correlationId);
            }
            catch (Exception ex)
            {
                var elapsedMs = GetElapsedMilliseconds(startTime);

                activity?.SetTag("http.status_code", StatusCodes.Status500InternalServerError);
                activity?.SetTag("http.response_time_ms", elapsedMs);
                activity?.SetTag("exception.message", ex.Message);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                _logger.LogError(
                    ex,
                    "HTTP request failed. Method: {Method}, Path: {Path}, ElapsedMs: {ElapsedMs}, CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    elapsedMs,
                    correlationId);

                throw;
            }
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
        {
            return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
        }
        return context.TraceIdentifier;
    }

    private static long GetElapsedMilliseconds(long startTimestamp)
    {
        var elapsed = Stopwatch.GetTimestamp() - startTimestamp;
        return (long)((double)elapsed / Stopwatch.Frequency * 1000);
    }
}
