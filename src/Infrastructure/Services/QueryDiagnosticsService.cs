using Application.Common.Interfaces;

namespace Infrastructure.Services;

  
public class QueryDiagnosticsService : IQueryDiagnosticsService
{
    private readonly List<QueryRecord> _queries = new();
    private readonly object _lock = new();
    private const int MaxStoredQueries = 1000;

    public event EventHandler<QueryRecord>? QueryExecuted;

    public void RecordQuery(QueryRecord record)
    {
        lock (_lock)
        {
            _queries.Add(record);

            if (_queries.Count > MaxStoredQueries)
            {
                _queries.RemoveRange(0, _queries.Count - MaxStoredQueries);
            }
        }

        QueryExecuted?.Invoke(this, record);
    }

    public IReadOnlyList<QueryRecord> GetRecentQueries()
    {
        lock (_lock)
        {
            return _queries
                .OrderByDescending(q => q.ExecutedAt)
                .Take(100)
                .ToList();
        }
    }

    public IReadOnlyList<QueryRecord> GetSlowQueries(int thresholdMs = 100)
    {
        lock (_lock)
        {
            return _queries
                .Where(q => q.DurationMs > thresholdMs)
                .OrderByDescending(q => q.DurationMs)
                .ToList();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _queries.Clear();
        }
    }
}
