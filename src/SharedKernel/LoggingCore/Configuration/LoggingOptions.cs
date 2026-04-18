// src/LoggingCore/Configuration/LoggingOptions.cs
namespace LoggingCore.Configuration;

public class LoggingOptions
{
    public const string SectionName = "LoggingSettings";

    public bool EnableApiLogging { get; set; } = true;
    public bool EnableExceptionLogging { get; set; } = true;
    public bool EnablePerformanceLogging { get; set; } = true;
    public bool ShowDetailsInProduction { get; set; } = false;
    public int BatchSize { get; set; } = 100;
    public int BatchIntervalMs { get; set; } = 1000;
    public int MaxQueueSize { get; set; } = 10000;
    public string[] ExcludedPaths { get; set; } = ["/health", "/metrics", "/favicon.ico"];
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public string[] SensitiveHeaders { get; set; } = ["Authorization", "Cookie", "X-Api-Key"];
}
