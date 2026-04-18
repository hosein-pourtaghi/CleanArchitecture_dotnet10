// src/LoggingCore/Services/AsyncLoggingService.cs
using System.Threading.Channels;
using LoggingCore.Configuration;
using LoggingCore.Data;
using LoggingCore.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoggingCore.Services;

public sealed class AsyncLoggingService : ILoggingService, IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LoggingOptions _options;
    private readonly ILogger<AsyncLoggingService> _logger;

    private readonly Channel<ApiLog> _apiLogChannel;
    private readonly Channel<ExceptionLog> _exceptionLogChannel;
    private readonly Channel<PerformanceMetric> _performanceMetricChannel;

    private readonly List<ApiLog> _apiLogBatch = new();
    private readonly List<ExceptionLog> _exceptionLogBatch = new();
    private readonly List<PerformanceMetric> _performanceMetricBatch = new();

    private readonly object _batchLock = new();
    private Timer? _batchTimer;
    private volatile bool _isProcessing;
    private volatile bool _disposed;
    private CancellationTokenSource? _internalCts;

    public AsyncLoggingService(
        IServiceScopeFactory scopeFactory,  // Changed from DbContext
        IOptions<LoggingOptions> options,
        ILogger<AsyncLoggingService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;

        var channelOptions = new BoundedChannelOptions(_options.MaxQueueSize)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _apiLogChannel = Channel.CreateBounded<ApiLog>(channelOptions);
        _exceptionLogChannel = Channel.CreateBounded<ExceptionLog>(channelOptions);
        _performanceMetricChannel = Channel.CreateBounded<PerformanceMetric>(channelOptions);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _batchTimer = new Timer(
            _ => ProcessBatches(),
            null,
            TimeSpan.FromMilliseconds(_options.BatchIntervalMs),
            TimeSpan.FromMilliseconds(_options.BatchIntervalMs));

        _ = ReadApiLogsAsync(_internalCts.Token);
        _ = ReadExceptionLogsAsync(_internalCts.Token);
        _ = ReadPerformanceMetricsAsync(_internalCts.Token);

        _logger.LogInformation("Async logging service started");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _batchTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _internalCts?.Cancel();

        _apiLogChannel.Writer.TryComplete();
        _exceptionLogChannel.Writer.TryComplete();
        _performanceMetricChannel.Writer.TryComplete();

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await FlushAsync(linkedCts.Token);
        }
        catch { }

        _logger.LogInformation("Async logging service stopped");
    }

    public Task LogApiRequestAsync(ApiLog log, CancellationToken cancellationToken = default)
    {
        if (!_options.EnableApiLogging)
            return Task.CompletedTask;

        _apiLogChannel.Writer.TryWrite(log);
        return Task.CompletedTask;
    }

    public Task LogExceptionAsync(ExceptionLog log, CancellationToken cancellationToken = default)
    {
        if (!_options.EnableExceptionLogging)
            return Task.CompletedTask;

        _exceptionLogChannel.Writer.TryWrite(log);
        return Task.CompletedTask;
    }

    public Task LogPerformanceMetricAsync(PerformanceMetric metric, CancellationToken cancellationToken = default)
    {
        if (!_options.EnablePerformanceLogging)
            return Task.CompletedTask;

        _performanceMetricChannel.Writer.TryWrite(metric);
        return Task.CompletedTask;
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            FlushApiLogsAsync(cancellationToken),
            FlushExceptionLogsAsync(cancellationToken),
            FlushPerformanceMetricsAsync(cancellationToken));
    }

    public int GetQueuedCount()
    {
        var count = 0;
        if (_apiLogChannel.Reader.CanCount)
            count += _apiLogChannel.Reader.Count;
        if (_exceptionLogChannel.Reader.CanCount)
            count += _exceptionLogChannel.Reader.Count;
        if (_performanceMetricChannel.Reader.CanCount)
            count += _performanceMetricChannel.Reader.Count;
        return count;
    }

    public bool IsQueueFull() => false;

    #region Private Methods

    private async Task ReadApiLogsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var log in _apiLogChannel.Reader.ReadAllAsync(cancellationToken))
            {
                bool shouldFlush;
                lock (_batchLock)
                {
                    _apiLogBatch.Add(log);
                    shouldFlush = _apiLogBatch.Count >= _options.BatchSize;
                }

                if (shouldFlush)
                    _ = FlushApiLogsAsyncSafe();
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _logger.LogError(ex, "Error reading API logs"); }
    }

    private async Task ReadExceptionLogsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var log in _exceptionLogChannel.Reader.ReadAllAsync(cancellationToken))
            {
                bool shouldFlush;
                lock (_batchLock)
                {
                    _exceptionLogBatch.Add(log);
                    shouldFlush = _exceptionLogBatch.Count >= _options.BatchSize;
                }

                if (shouldFlush)
                    _ = FlushExceptionLogsAsyncSafe();
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _logger.LogError(ex, "Error reading exception logs"); }
    }

    private async Task ReadPerformanceMetricsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var metric in _performanceMetricChannel.Reader.ReadAllAsync(cancellationToken))
            {
                bool shouldFlush;
                lock (_batchLock)
                {
                    _performanceMetricBatch.Add(metric);
                    shouldFlush = _performanceMetricBatch.Count >= _options.BatchSize;
                }

                if (shouldFlush)
                    _ = FlushPerformanceMetricsAsyncSafe();
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _logger.LogError(ex, "Error reading performance metrics"); }
    }

    private void ProcessBatches()
    {
        if (_isProcessing || _disposed)
            return;
        _isProcessing = true;

        try
        {
            _ = Task.Run(async () =>
            {
                try
                { await FlushAsync(); }
                catch (Exception ex) { _logger.LogError(ex, "Error during batch processing"); }
            });
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task FlushApiLogsAsyncSafe(CancellationToken cancellationToken = default)
    {
        try
        { await FlushApiLogsAsync(cancellationToken); }
        catch (Exception ex) { _logger.LogError(ex, "Error flushing API logs"); }
    }

    private async Task FlushExceptionLogsAsyncSafe(CancellationToken cancellationToken = default)
    {
        try
        { await FlushExceptionLogsAsync(cancellationToken); }
        catch (Exception ex) { _logger.LogError(ex, "Error flushing exception logs"); }
    }

    private async Task FlushPerformanceMetricsAsyncSafe(CancellationToken cancellationToken = default)
    {
        try
        { await FlushPerformanceMetricsAsync(cancellationToken); }
        catch (Exception ex) { _logger.LogError(ex, "Error flushing performance metrics"); }
    }

    private async Task FlushApiLogsAsync(CancellationToken cancellationToken = default)
    {
        List<ApiLog> batch;

        lock (_batchLock)
        {
            if (_apiLogBatch.Count == 0)
                return;
            batch = new List<ApiLog>(_apiLogBatch);
            _apiLogBatch.Clear();
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();

        try
        {
            await dbContext.ApiLogs.AddRangeAsync(batch, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Flushed {Count} API logs", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush {Count} API logs", batch.Count);
        }
    }

    private async Task FlushExceptionLogsAsync(CancellationToken cancellationToken = default)
    {
        List<ExceptionLog> batch;

        lock (_batchLock)
        {
            if (_exceptionLogBatch.Count == 0)
                return;
            batch = new List<ExceptionLog>(_exceptionLogBatch);
            _exceptionLogBatch.Clear();
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();

        try
        {
            await dbContext.ExceptionLogs.AddRangeAsync(batch, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Flushed {Count} exception logs", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush {Count} exception logs", batch.Count);
        }
    }

    private async Task FlushPerformanceMetricsAsync(CancellationToken cancellationToken = default)
    {
        List<PerformanceMetric> batch;

        lock (_batchLock)
        {
            if (_performanceMetricBatch.Count == 0)
                return;
            batch = new List<PerformanceMetric>(_performanceMetricBatch);
            _performanceMetricBatch.Clear();
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();

        try
        {
            await dbContext.PerformanceMetrics.AddRangeAsync(batch, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Flushed {Count} performance metrics", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush {Count} performance metrics", batch.Count);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _batchTimer?.Dispose();
        _internalCts?.Dispose();
    }

    #endregion
}
