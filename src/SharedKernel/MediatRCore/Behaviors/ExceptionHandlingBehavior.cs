// src/MediatRCore/Behaviors/ExceptionHandlingBehavior.cs
using System.Diagnostics;
using SharedKernel.LoggingCore.Entities;
using SharedKernel.LoggingCore.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;
using Microsoft.AspNetCore.Http;

namespace SharedKernel.MediatRCore.Behaviors;
 

/// <summary>
/// MediatR pipeline behavior for handling exceptions from handlers
/// </summary> 
public sealed class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILoggingService _loggingService;
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExceptionHandlingBehavior(
        ILoggingService loggingService,
        ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger,
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
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            var traceId = Activity.Current?.Id ?? _httpContextAccessor.HttpContext?.TraceIdentifier;

            _logger.LogError(
                ex,
                "Exception in {RequestName}. TraceId: {TraceId}",
                requestName,
                traceId);

            // Log exception (non-blocking)
            await LogExceptionAsync(ex, requestName, traceId);

            // Re-throw to let the global middleware handle the response
            throw;
        }
    }

    private async Task LogExceptionAsync(Exception exception, string requestName, string? traceId)
    {
        try
        {
            var log = new ExceptionLog
            {
                TraceId = traceId ?? Guid.NewGuid().ToString(),
                CorrelationId = _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-Id"].FirstOrDefault(),
                ExceptionType = exception.GetType().Name,
                Code = exception is BaseException baseEx ? baseEx.Code : "MEDIATR.ERROR",
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException?.ToString(),
                UserId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value,
                RequestPath = _httpContextAccessor.HttpContext?.Request.Path.Value,
                RequestMethod = requestName,
                Timestamp = DateTime.UtcNow,
                MachineName = Environment.MachineName,
                IsHandled = false,
                HandledBy = "ExceptionHandlingBehavior"
            };

            await _loggingService.LogExceptionAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log exception from MediatR behavior");
        }
    }
}
