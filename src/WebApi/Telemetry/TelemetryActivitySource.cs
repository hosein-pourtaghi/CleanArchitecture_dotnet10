using System.Diagnostics;

namespace WebApi.Telemetry;

/// <summary>
/// Provides a centralized ActivitySource for OpenTelemetry tracing across the WebApi project.
/// </summary>
public static class TelemetryActivitySource
{
    public static readonly ActivitySource Instance = new(
        "WebApi",
        "1.0.0");
}
