// src/LoggingLibrary/Telemetry/TelemetryActivitySource.cs
// ============================================================
// PURPOSE: Centralized activity source for distributed tracing
// ============================================================

using System.Diagnostics;

namespace SharedKernel.LoggingCore.Telemetry;

/// <summary>
/// Singleton activity source for OpenTelemetry tracing.
/// </summary>
public sealed class TelemetryActivitySource
{
    private static readonly Lazy<TelemetryActivitySource> _instance =
        new(() => new TelemetryActivitySource());

    /// <summary>
    /// Gets the singleton instance of TelemetryActivitySource.
    /// </summary>
    public static TelemetryActivitySource Instance => _instance.Value;

    /// <summary>
    /// Gets the ActivitySource instance for creating activities.
    /// </summary>
    public ActivitySource ActivitySource { get; }

    /// <summary>
    /// Gets the name of the activity source.
    /// </summary>
    public string Name { get; }

    private TelemetryActivitySource()
    {
        Name = "SharedKernel.Logging";
        ActivitySource = new ActivitySource(Name);
    }

    /// <summary>
    /// Starts a new activity with the specified name.
    /// </summary>
    public Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(name, kind);
    }

    /// <summary>
    /// Starts a new activity with the specified name and parent context.
    /// </summary>
    public Activity? StartActivity(string name, ActivityKind kind, ActivityContext parentContext)
    {
        return ActivitySource.StartActivity(name, kind, parentContext);
    }
}
