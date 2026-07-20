//// src/LoggingCore/Interceptors/QueryLoggingInterceptor.cs
//using System.Data.Common;
//using System.Diagnostics;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using SharedKernel.LoggingCore.Configuration;
//using SharedKernel.LoggingCore.Entities;
//using SharedKernel.LoggingCore.Services;

//namespace SharedKernel.LoggingCore.Interceptors;

//public class QueryLoggingInterceptor : DbCommandInterceptor
//{
//    private readonly ILoggingService _loggingService;
//    private readonly LoggingOptions _options;
//    private readonly ILogger<QueryLoggingInterceptor> _logger;

//    public QueryLoggingInterceptor(
//        ILoggingService loggingService,
//        IOptions<LoggingOptions> options,
//        ILogger<QueryLoggingInterceptor> logger)
//    {
//        _loggingService = loggingService;
//        _options = options.Value;
//        _logger = logger;
//    }

//    // Override the reader executed method
//    public override DbDataReader ReaderExecuted(
//        DbCommand command,
//        CommandExecutedEventData eventData,
//        DbDataReader result)
//    {
//        LogQuery(command, eventData.Duration);
//        return result;
//    }

//    // Override non-query executed
//    public override int NonQueryExecuted(
//        DbCommand command,
//        CommandExecutedEventData eventData,
//        int result)
//    {
//        LogQuery(command, eventData.Duration);
//        return result;
//    }

//    // Override scalar executed
//    public override object? ScalarExecuted(
//        DbCommand command,
//        CommandExecutedEventData eventData,
//        object? result)
//    {
//        LogQuery(command, eventData.Duration);
//        return result;
//    }

//    private void LogQuery(DbCommand command, TimeSpan? duration)
//    {
//        if (!duration.HasValue || !_options.EnableQueryLogging)
//            return;

//        var durationMs = (int)duration.Value.TotalMilliseconds;
//        var isSlow = durationMs > _options.SlowQueryThresholdMs;

//        if (isSlow)
//        {
//            _logger.LogWarning(
//                "SLOW QUERY: {Duration}ms - {Sql}",
//                durationMs,
//                TruncateSql(command.CommandText));
//        }

//        var queryLog = new QueryLog
//        {
//            TraceId = Activity.Current?.Id ?? Guid.NewGuid().ToString(),
//            Sql = command.CommandText,
//            Parameters = FormatParameters(command.Parameters),
//            DurationMs = durationMs,
//            CommandType = command.CommandType.ToString(),
//            Database = command.Connection?.Database,
//            Timestamp = DateTime.UtcNow,
//            IsSlowQuery = isSlow,
//            MachineName = Environment.MachineName
//        };

//        _loggingService.LogQueryAsync(queryLog);
//    }

//    private static string? FormatParameters(DbParameterCollection? parameters)
//    {
//        if (parameters == null || parameters.Count == 0)
//            return null;

//        return string.Join(", ", parameters.Cast<DbParameter>().Select(p =>
//            $"{p.ParameterName}={p.Value}"));
//    }

//    private static string TruncateSql(string sql) =>
//        sql.Length > 500 ? sql[..500] + "..." : sql;
//}
