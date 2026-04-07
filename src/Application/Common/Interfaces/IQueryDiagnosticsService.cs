using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;
 

//public interface IQueryDiagnosticsService
//{
//    void RecordQuery(QueryRecord record);
//    IReadOnlyList<QueryRecord> GetRecentQueries();
//    IReadOnlyList<QueryRecord> GetSlowQueries(int thresholdMs = 100);
//    void Clear();
//    event EventHandler<QueryRecord>? QueryExecuted;
//}

//public class QueryRecord
//{
//    public string Sql { get; set; } = string.Empty;
//    public string? Parameters { get; set; }
//    public long DurationMs { get; set; }  // Changed from Duration struct
//    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
//    public string? CommandType { get; set; }
//}
