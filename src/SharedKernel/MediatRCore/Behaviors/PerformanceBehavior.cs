// src/MediatRCore/Behaviors/PerformanceBehavior.cs
using MediatR;
using System.Diagnostics;
using LoggingCore.Services;
using LoggingCore.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace MediatRCore.Behaviors;

/// <summary>
/// MediatR pipeline behavior for tracking performance metrics
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILoggingService _loggingService;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PerformanceBehavior(
        ILoggingService loggingService,
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _loggingService = loggingService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        var traceId = Activity.Current?.Id ?? _httpContextAccessor.HttpContext?.TraceIdentifier;

        try
        {
            _logger.LogDebug("Executing {RequestName}", requestName);
            var response = await next();
            return response;
        }
        finally
        {
            stopwatch.Stop();
            var durationMs = stopwatch.ElapsedMilliseconds;

            // Log performance metric (non-blocking)
            await LogPerformanceMetricAsync(requestName, traceId, durationMs);

            if (durationMs > 1000)
            {
                _logger.LogWarning(
                    "Slow operation detected: {RequestName} took {Duration}ms",
                    requestName,
                    durationMs);
            }
            else
            {
                _logger.LogDebug(
                    "Completed {RequestName} in {Duration}ms",
                    requestName,
                    durationMs);
            }
        }
    }

    private async Task LogPerformanceMetricAsync(string operationName, string? traceId, long durationMs)
    {
        try
        {
            var metric = new PerformanceMetric
            {
                TraceId = traceId ?? Guid.NewGuid().ToString(),
                OperationName = operationName,
                UserId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value,
                RequestPath = _httpContextAccessor.HttpContext?.Request.Path.Value,
                DurationMs = durationMs,
                MemoryUsedBytes = GC.GetTotalMemory(false),
                Timestamp = DateTime.UtcNow,
                IsSlowOperation = durationMs > 1000,
                MachineName = Environment.MachineName
            };

            await _loggingService.LogPerformanceMetricAsync(metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log performance metric");
        }
    }
}
