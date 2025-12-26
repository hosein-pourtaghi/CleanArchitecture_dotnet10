using System.Diagnostics;

namespace Web.Api.Telemetry;

/// <summary>
/// Provides a centralized ActivitySource for OpenTelemetry tracing across the Web.Api project.
/// </summary>
public static class TelemetryActivitySource
{
    public static readonly ActivitySource Instance = new(
        "Web.Api",
        "1.0.0");
}
