using System.Collections.Concurrent;

namespace LoadSimulator.Infrastructure;

/// <summary>
/// Collects and aggregates simulation metrics
/// </summary>
public class SimulationMetricsService
{
    private readonly ConcurrentBag<long> _responseTimes = new();
    private readonly ConcurrentDictionary<string, int> _errorCounts = new();
    private long _totalOrders = 0;
    private long _successfulOrders = 0;
    private long _failedOrders = 0;
    private int _successfulUsers = 0;
    private int _failedUsers = 0;
    private readonly object _lock = new object();

    public void RecordResponseTime(long milliseconds) => 
        _responseTimes.Add(milliseconds);

    public void RecordSuccessfulOrder()
    {
        Interlocked.Increment(ref _successfulOrders);
        Interlocked.Increment(ref _totalOrders);
    }

    public void RecordFailedOrder()
    {
        Interlocked.Increment(ref _failedOrders);
        Interlocked.Increment(ref _totalOrders);
    }

    public void RecordSuccessfulUser() => 
        Interlocked.Increment(ref _successfulUsers);

    public void RecordFailedUser() => 
        Interlocked.Increment(ref _failedUsers);

    public void RecordError(string errorType)
    {
        _errorCounts.AddOrUpdate(errorType, 1, (_, count) => count + 1);
    }

    public void Reset()
    {
        lock (_lock)
        {
            _responseTimes.Clear();
            _errorCounts.Clear();
            _totalOrders = 0;
            _successfulOrders = 0;
            _failedOrders = 0;
            _successfulUsers = 0;
            _failedUsers = 0;
        }
    }

    public MetricsSnapshot GetSnapshot(TimeSpan duration)
    {
        lock (_lock)
        {
            var responseTimes = _responseTimes.ToList();
            responseTimes.Sort();

            return new MetricsSnapshot
            {
                TotalOrders = _totalOrders,
                SuccessfulOrders = _successfulOrders,
                FailedOrders = _failedOrders,
                SuccessfulUsers = _successfulUsers,
                FailedUsers = _failedUsers,
                AverageResponseTime = responseTimes.Count > 0 
                    ? responseTimes.Average() 
                    : 0,
                MinResponseTime = responseTimes.Count > 0 
                    ? responseTimes.First() 
                    : 0,
                MaxResponseTime = responseTimes.Count > 0 
                    ? responseTimes.Last() 
                    : 0,
                P95ResponseTime = GetPercentile(responseTimes, 95),
                P99ResponseTime = GetPercentile(responseTimes, 99),
                OrdersPerSecond = duration.TotalSeconds > 0 
                    ? _successfulOrders / duration.TotalSeconds 
                    : 0,
                ErrorCounts = new Dictionary<string, int>(_errorCounts),
                TotalErrors = _errorCounts.Values.Sum()
            };
        }
    }

    private static long GetPercentile(List<long> sortedValues, int percentile)
    {
        if (sortedValues.Count == 0)
            return 0;

        var index = (int)Math.Ceiling((percentile / 100.0) * sortedValues.Count) - 1;
        return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
    }

    public class MetricsSnapshot
    {
        public long TotalOrders { get; set; }
        public long SuccessfulOrders { get; set; }
        public long FailedOrders { get; set; }
        public int SuccessfulUsers { get; set; }
        public int FailedUsers { get; set; }
        public double AverageResponseTime { get; set; }
        public long MinResponseTime { get; set; }
        public long MaxResponseTime { get; set; }
        public long P95ResponseTime { get; set; }
        public long P99ResponseTime { get; set; }
        public double OrdersPerSecond { get; set; }
        public Dictionary<string, int> ErrorCounts { get; set; } = new();
        public int TotalErrors { get; set; }
    }
}
