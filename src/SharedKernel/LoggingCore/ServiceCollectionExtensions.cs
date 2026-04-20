// src/LoggingLibrary/Extensions/ServiceCollectionExtensions.cs
// ============================================================
// PURPOSE: High-level logging library configuration extensions
// ============================================================

using LoggingLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.LoggingCore.Configuration;
using SharedKernel.LoggingCore.DependencyInjection;
using SharedKernel.LoggingCore.Services;

namespace SharedKernel.LoggingCore;

/// <summary>
/// Extension methods for configuring the logging library.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the logging library services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="configure">Optional action to configure logging options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoggingLibrary(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<LoggingOptions>? configure = null)
    {
        // Configure options - use provided action or empty lambda
        services.Configure(configure ?? (_ => { }));

        // Add logging options from configuration section
        services.AddLoggingOptions(configuration);

        // Core services
        services.AddHttpContextAccessor();
        services.AddScoped<ITraceIdAccessor, TraceIdAccessor>();

        // Configure and add logging services with recommended settings
        services.AddLoggingServices(options =>
        {
            options.EnableApiLogging = true;
            options.EnableExceptionLogging = true;
            options.EnablePerformanceLogging = true;
            options.EnableQueryLogging = true;  // Only in dev environment
            options.SlowQueryThresholdMs = 500;  // Log queries > 500ms
            options.ShowDetailsInProduction = true;
            options.BatchSize = 100;
            options.BatchIntervalMs = 1000;
            options.MaxQueueSize = 10000;
        });

        return services;
    }

    /// <summary>
    /// Adds logging options from configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoggingOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration section to LoggingOptions
        services.Configure<LoggingOptions>(
            configuration.GetSection("LoggingSettings"));

        return services;
    }

    /// <summary>
    /// Uses the logging library middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseLoggingLibrary(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
