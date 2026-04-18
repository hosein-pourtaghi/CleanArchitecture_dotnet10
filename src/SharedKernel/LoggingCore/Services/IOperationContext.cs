// src/LoggingLibrary/Services/IOperationContext.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.LoggingCore.Services;
 

/// <summary>
/// Tracks operation execution for logging purposes.
/// Use this instead of IPipelineBehavior for MediatR handlers.
/// </summary>
public interface IOperationContext
{
    /// <summary>
    /// Start tracking an operation
    /// </summary>
    void BeginOperation(string operationName, string? operationType = null);

    /// <summary>
    /// End tracking the current operation
    /// </summary>
    void EndOperation();

    /// <summary>
    /// End tracking with a result
    /// </summary>
    void EndOperation(object? result);

    /// <summary>
    /// End tracking with an exception
    /// </summary>
    void EndOperation(Exception exception);

    /// <summary>
    /// Get current operation info
    /// </summary>
    OperationInfo? CurrentOperation { get; }

    /// <summary>
    /// Get all completed operations in this request
    /// </summary>
    IReadOnlyList<OperationInfo> CompletedOperations { get; }
}

public class OperationInfo
{
    public string TraceId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public long DurationMs { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsSlowOperation { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; }
    public long MemoryUsedBytes { get; set; }
    public string? UserId { get; set; }
    public string? RequestPath { get; set; }
}
