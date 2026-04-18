// src/YourApi/Middleware/ExceptionHandlingMiddleware.cs
using System.Diagnostics;
using LoggingCore.Entities;
using LoggingCore.Mapping;
using LoggingCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Exceptions;

namespace LoggingCore.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggingService _loggingService;
    private readonly LoggingCore.Configuration.LoggingOptions _options;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILoggingService loggingService,
        IOptions<LoggingCore.Configuration.LoggingOptions> options,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _loggingService = loggingService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? traceId;

        context.Items["TraceId"] = traceId;
        context.Items["CorrelationId"] = correlationId;

        Exception? capturedException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            capturedException = ex;
            await HandleExceptionAsync(context, ex, traceId, correlationId, stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            if (_options.EnableApiLogging && !IsExcludedPath(context.Request.Path))
            {
                await LogApiRequestAsync(context, traceId, correlationId, stopwatch.ElapsedMilliseconds ,capturedException);
            }
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, string traceId, string correlationId, long durationMs)
    {
        var showDetails = _options.ShowDetailsInProduction;//|| context.Request.IsDevelopment();

        if (ex is BaseException baseEx)
            baseEx.CorrelationId = traceId;

        await LogExceptionAsync(context, ex, traceId, correlationId);

        var statusCode = ex.GetStatusCode();
        var response = ex.ToResponse(showDetails);
        response.TraceId = traceId;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.Body.WriteAsync(response.ToJsonBytes());

        _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}, Path: {Path}", traceId, context.Request.Path);
    }

    private async Task LogApiRequestAsync(HttpContext context, string traceId, string correlationId, long durationMs, Exception? exception)
    {
        try
        {
            var log = new ApiLog
            {
                TraceId = traceId,
                CorrelationId = correlationId,
                Method = context.Request.Method,
                Path = context.Request.Path.Value ?? string.Empty,
                QueryString = context.Request.QueryString.Value,
                UserId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("uid")?.Value,
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                StatusCode = context.Response.StatusCode,
                RequestDurationMs = durationMs,
                RequestTimestamp = DateTime.UtcNow.AddMilliseconds(-durationMs),
                ResponseTimestamp = DateTime.UtcNow,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                MachineName = Environment.MachineName,
                // Capture exception info
                ExceptionMessage = exception?.Message,
                ExceptionType = exception?.GetType().Name
            };

            await _loggingService.LogApiRequestAsync(log);
        }
        catch { /* swallow */ }
    }

    private async Task LogExceptionAsync(HttpContext context, Exception ex, string traceId, string correlationId)
    {
        try
        {
            var log = new LoggingCore.Entities.ExceptionLog
            {
                TraceId = traceId,
                CorrelationId = correlationId,
                ExceptionType = ex.GetType().Name,
                Code = ex is BaseException baseEx ? baseEx.Code : "UNKNOWN",
                Message = ex.Message,
                StackTrace = _options.ShowDetailsInProduction ? ex.StackTrace : null,
                InnerException = ex.InnerException?.ToString(),
                UserId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("uid")?.Value,
                RequestPath = context.Request.Path.Value,
                RequestMethod = context.Request.Method,
                Timestamp = DateTime.UtcNow,
                MachineName = Environment.MachineName,
                IsHandled = true,
                HandledBy = "ExceptionHandlingMiddleware"
            };

            await _loggingService.LogExceptionAsync(log);
        }
        catch { /* swallow */ }
    }

    private bool IsExcludedPath(string path)
    {
        return _options.ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}
