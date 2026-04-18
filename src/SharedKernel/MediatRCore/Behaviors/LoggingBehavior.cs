// src/MediatRCore/Behaviors/LoggingBehavior.cs
using System.Diagnostics;
using System.Text.Json;
using LoggingCore.Entities;
using LoggingCore.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MediatRCore.Behaviors;
 

/// <summary>
/// MediatR pipeline behavior for logging requests and responses
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILoggingService _loggingService;
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        MaxDepth = 3 // Limit depth to avoid huge logs
    };

    public LoggingBehavior(
        ILoggingService loggingService,
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
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
        var traceId = Activity.Current?.Id ?? _httpContextAccessor.HttpContext?.TraceIdentifier;

        _logger.LogDebug(
            "MediatR Request: {RequestName}, TraceId: {TraceId}, Request: {Request}",
            requestName,
            traceId,
            SerializeRequest(request));

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        TResponse response;

        try
        {
            response = await next();
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogDebug(
                "MediatR Response: {RequestName}, TraceId: {TraceId}, Duration: {Duration}ms",
                requestName,
                traceId,
                stopwatch.ElapsedMilliseconds);
        }

        return response;
    }

    private string? SerializeRequest(TRequest request)
    {
        try
        {
            if (request == null)
                return null;

            // Skip serializing large requests
            var json = JsonSerializer.Serialize(request, JsonOptions);
            return json.Length > 10000 ? json[..10000] + "... [truncated]" : json;
        }
        catch
        {
            return "[Serialization failed]";
        }
    }
}
