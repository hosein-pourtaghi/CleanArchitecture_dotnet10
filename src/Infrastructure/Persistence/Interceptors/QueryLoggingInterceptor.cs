using System.Data.Common;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Interceptors;

///// <summary>
///// Interceptor for logging EF Core queries
///// </summary>
//public class QueryLoggingInterceptor : DbCommandInterceptor
//{
//    private readonly IQueryDiagnosticsService _diagnosticsService;
//    private readonly ILogger<QueryLoggingInterceptor> _logger;

//    public QueryLoggingInterceptor(
//        IQueryDiagnosticsService diagnosticsService,
//        ILogger<QueryLoggingInterceptor> logger)
//    {
//        _diagnosticsService = diagnosticsService;
//        _logger = logger;
//    }

//    public override void CommandExecuted(
//        DbCommand command,
//        CommandExecutedEventData eventData)
//    {
//        var duration = eventData.Duration;

//        if (duration.HasValue)
//        {
//            var record = new QueryRecord
//            {
//                Sql = command.CommandText,
//                Parameters = FormatParameters(command.Parameters),
//                DurationMs = duration.Value.Milliseconds,
//                CommandType = command.CommandType.ToString()
//            };

//            _diagnosticsService.RecordQuery(record);

//            if (duration.Value.TotalMilliseconds > 1000)
//            {
//                _logger.LogWarning(
//                    "SLOW QUERY: {Duration}ms - {Sql}",
//                    duration.Value.TotalMilliseconds,
//                    TruncateSql(command.CommandText));
//            }
//        }

//        base.CommandExecuted(command, eventData);
//    }

//    public override Task CommandExecutedAsync(
//        DbCommand command,
//        CommandExecutedEventData eventData,
//        CancellationToken cancellationToken = default)
//    {
//        var duration = eventData.Duration;

//        if (duration.HasValue)
//        {
//            var record = new QueryRecord
//            {
//                Sql = command.CommandText,
//                Parameters = FormatParameters(command.Parameters),
//                DurationMs = duration.Value.Milliseconds,
//                CommandType = command.CommandType.ToString()
//            };

//            _diagnosticsService.RecordQuery(record);

//            if (duration.Value.TotalMilliseconds > 1000)
//            {
//                _logger.LogWarning(
//                    "SLOW QUERY: {Duration}ms - {Sql}",
//                    duration.Value.TotalMilliseconds,
//                    TruncateSql(command.CommandText));
//            }
//        }

//        return base.CommandExecutedAsync(command, eventData, cancellationToken);
//    }

//    private static string? FormatParameters(DbParameterCollection? parameters)
//    {
//        if (parameters == null || parameters.Count == 0)
//            return null;

//        var paramStrings = new List<string>();
//        foreach (DbParameter param in parameters)
//        {
//            paramStrings.Add($"{param.ParameterName}={param.Value}");
//        }

//        return string.Join(", ", paramStrings);
//    }

//    private static string TruncateSql(string sql)
//    {
//        return sql.Length > 200 ? sql[..200] + "..." : sql;
//    }
//}
