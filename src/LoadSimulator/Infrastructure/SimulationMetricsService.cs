namespace LoadSimulator.Infrastructure;

/// <summary>
/// Collects and aggregates simulation metrics
/// </summary>
public class SimulationMetricsService
{
    private readonly ConcurrentBag<long> _responseTimes = new();
    private readonly ConcurrentDictionary<string, int> _errorCounts = new();
    private long _totalCarts;
    private long _successfulCarts;
    private long _failedCarts;
    private int _successfulUsers;
    private int _failedUsers;
    private readonly object _lock = new object();

    public void RecordResponseTime(long milliseconds) =>
        _responseTimes.Add(milliseconds);

    public void RecordSuccessfulCart()
    {
        Interlocked.Increment(ref _successfulCarts);
        Interlocked.Increment(ref _totalCarts);
    }

    public void RecordFailedCart()
    {
        Interlocked.Increment(ref _failedCarts);
        Interlocked.Increment(ref _totalCarts);
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
            _totalCarts = 0;
            _successfulCarts = 0;
            _failedCarts = 0;
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
                TotalCarts = _totalCarts,
                SuccessfulCarts = _successfulCarts,
                FailedCarts = _failedCarts,
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
                CartsPerSecond = duration.TotalSeconds > 0
                    ? _successfulCarts / duration.TotalSeconds
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
        public long TotalCarts { get; set; }
        public long SuccessfulCarts { get; set; }
        public long FailedCarts { get; set; }
        public int SuccessfulUsers { get; set; }
        public int FailedUsers { get; set; }
        public double AverageResponseTime { get; set; }
        public long MinResponseTime { get; set; }
        public long MaxResponseTime { get; set; }
        public long P95ResponseTime { get; set; }
        public long P99ResponseTime { get; set; }
        public double CartsPerSecond { get; set; }
        public Dictionary<string, int> ErrorCounts { get; set; } = new();
        public int TotalErrors { get; set; }
    }
}
