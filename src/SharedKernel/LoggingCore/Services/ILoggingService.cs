// src/LoggingCore/Services/ILoggingService.cs
using LoggingCore.Entities;

namespace LoggingCore.Services;

public interface ILoggingService
{
    // Non-blocking API - returns immediately
    Task LogApiRequestAsync(ApiLog log, CancellationToken cancellationToken = default);
    Task LogExceptionAsync(ExceptionLog log, CancellationToken cancellationToken = default);
    Task LogPerformanceMetricAsync(PerformanceMetric metric, CancellationToken cancellationToken = default);
    Task LogQueryAsync(QueryLog log, CancellationToken cancellationToken = default);  

    // Batch operations for bulk inserts
    Task FlushAsync(CancellationToken cancellationToken = default);

    // Queue statistics
    int GetQueuedCount();
    bool IsQueueFull();
}
