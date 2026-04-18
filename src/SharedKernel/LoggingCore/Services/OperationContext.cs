// src/LoggingLibrary/Services/OperationContext.cs
using System.Diagnostics;
using LoggingCore.Configuration;
using Microsoft.AspNetCore.Http;

namespace SharedKernel.LoggingCore.Services;

public class OperationContext : IOperationContext
{
    private readonly ITraceIdAccessor _traceIdAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggingOptions _options;
    private readonly Stopwatch _requestStopwatch;

    private OperationInfo? _currentOperation;
    private readonly List<OperationInfo> _completedOperations = new();
    private readonly long _requestStartMemory;

    public OperationContext(
        ITraceIdAccessor traceIdAccessor,
        IHttpContextAccessor httpContextAccessor,
        LoggingOptions options)
    {
        _traceIdAccessor = traceIdAccessor;
        _httpContextAccessor = httpContextAccessor;
        _options = options;
        _requestStopwatch = Stopwatch.StartNew();
        _requestStartMemory = GC.GetTotalMemory(false);
    }

    public OperationInfo? CurrentOperation => _currentOperation;
    public IReadOnlyList<OperationInfo> CompletedOperations => _completedOperations.AsReadOnly();

    public void BeginOperation(string operationName, string? operationType = null)
    {
        // End any previous operation
        if (_currentOperation != null)
        {
            EndOperation();
        }

        var context = _httpContextAccessor.HttpContext;

        _currentOperation = new OperationInfo
        {
            TraceId = _traceIdAccessor.TraceId,
            CorrelationId = _traceIdAccessor.CorrelationId,
            Name = operationName,
            Type = operationType,
            StartTime = DateTime.UtcNow,
            UserId = context?.User?.FindFirst("sub")?.Value ?? context?.User?.FindFirst("uid")?.Value,
            RequestPath = context?.Request.Path.Value
        };
    }

    public void EndOperation()
    {
        CompleteOperation(success: true, exception: null);
    }

    public void EndOperation(object? result)
    {
        CompleteOperation(success: true, exception: null);
    }

    public void EndOperation(Exception exception)
    {
        CompleteOperation(success: false, exception: exception);
    }

    private void CompleteOperation(bool success, Exception? exception)
    {
        if (_currentOperation == null)
            return;

        _currentOperation.EndTime = DateTime.UtcNow;
        _currentOperation.DurationMs = (long)_currentOperation.EndTime.Subtract(_currentOperation.StartTime).TotalMilliseconds;
        _currentOperation.IsSuccess = success;
        _currentOperation.IsSlowOperation = _currentOperation.DurationMs > _options.SlowQueryThresholdMs;
        _currentOperation.MemoryUsedBytes = GC.GetTotalMemory(false) - _requestStartMemory;

        if (exception != null)
        {
            _currentOperation.ErrorMessage = exception.Message;
            _currentOperation.ErrorType = exception.GetType().Name;
        }

        _completedOperations.Add(_currentOperation);
        _currentOperation = null;
    }
}
