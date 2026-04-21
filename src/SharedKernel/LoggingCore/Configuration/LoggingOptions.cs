// src/LoggingCore/Configuration/LoggingOptions.cs
namespace SharedKernel.LoggingCore.Configuration;

public class LoggingOptions
{
    public const string SectionName = "LoggingSettings";

    public bool EnableApiLogging { get; set; } = true;
    public bool EnableExceptionLogging { get; set; } = true;
    public bool EnablePerformanceLogging { get; set; } = true;
    public bool EnableQueryLogging { get; set; } = false;
    public int SlowQueryThresholdMs { get; set; } = 1000;    
    public bool ShowDetailsInProduction { get; set; } = false;
    public int BatchSize { get; set; } = 100;
    public int BatchIntervalMs { get; set; } = 1000;
    public int MaxQueueSize { get; set; } = 10000;
    public string[] ExcludedPaths { get; set; } = ["/health", "/metrics", "/favicon.ico"];
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public string[] SensitiveHeaders { get; set; } = ["Authorization", "Cookie", "X-Api-Key"];
  
    /// <summary>
    /// Gets or sets the application name for logging.
    /// </summary>
    public string ApplicationName { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets the Serilog configuration section name.
    /// </summary>
    public string SerilogSectionName { get; set; } = "Serilog";

    /// <summary>
    /// Gets or sets the OpenTelemetry service name.
    /// </summary>
    public string OtelServiceName { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets whether to enable OpenTelemetry tracing.
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable OpenTelemetry metrics.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;
}

/// <summary>
/// Options for Serilog configuration.
/// </summary>
public class SerilogOptions
{
    public string MinimumLevel { get; set; } = "Information";
    public bool EnrichFromLogContext { get; set; } = true;
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// Options for OpenTelemetry configuration.
/// </summary>
public class OpenTelemetryOptions
{
    public string ServiceName { get; set; } = "Unknown";
    public string? OtlpEndpoint { get; set; }
    public bool EnableAspNetCoreInstrumentation { get; set; } = true;
    public bool EnableHttpClientInstrumentation { get; set; } = true;
    public bool EnableRuntimeInstrumentation { get; set; } = true;
    public bool EnableTracing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
}
