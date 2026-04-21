// src/LoggingLibrary/DependencyInjection/LoggingServiceExtensions.cs
// ============================================================
// PURPOSE: Extension methods for registering logging services
// ============================================================

using LoggingLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using SharedKernel.LoggingCore.Configuration;
using SharedKernel.LoggingCore.Data;
using SharedKernel.LoggingCore.Services;
using SharedKernel.LoggingCore.Telemetry;

namespace SharedKernel.LoggingCore.DependencyInjection;

/// <summary>
/// Extension methods for configuring logging services in the DI container.
/// </summary>
public static class LoggingServiceExtensions
{
    #region Core Logging Services

    /// <summary>
    /// Adds logging services to the service collection.
    /// </summary>
    public static IServiceCollection AddLoggingServices(
        this IServiceCollection services,
        Action<LoggingOptions>? configureOptions = null)
    {
        // Configure options using Options pattern
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.AddOptions<LoggingOptions>()
                .Configure(options =>
                {
                    options.EnableApiLogging = true;
                    options.EnableExceptionLogging = true;
                    options.EnablePerformanceLogging = true;
                    options.EnableQueryLogging = true;
                    options.SlowQueryThresholdMs = 1000;
                    options.BatchSize = 100;
                    options.BatchIntervalMs = 1000;
                    options.MaxQueueSize = 10000;
                });
        }

        // Register logging service as singleton
        services.AddSingleton<ILoggingService, LoggingService>();

        // Register as hosted service for background processing
        services.AddHostedService(sp => (LoggingService)sp.GetRequiredService<ILoggingService>());

        return services;
    }

    /// <summary>
    /// Adds the logging database context.
    /// </summary>
    public static IServiceCollection AddLoggingDbContext(
        this IServiceCollection services,
        string connectionString,
        bool useInMemory = false)
    {
        if (useInMemory)
        {
            //// Use in-memory database for testing
            //services.AddDbContext<LoggingDbContext>(options =>
            //    options.UseInMemoryDatabase("LoggingDb"));
        }
        else
        {
            services.AddDbContext<LoggingDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                        sqlOptions.CommandTimeout(30);
                    }));
        }

        return services;
    }

    /// <summary>
    /// Ensures the logging database is created.
    /// </summary>
    public static async Task InitializeLoggingDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    #endregion

    #region Unified AddLoggingLibrary (Combined from ServiceCollectionExtensions)

    /// <summary>
    /// Adds the complete logging library with Serilog, OpenTelemetry, and all services.
    /// This is the main entry point for configuring the logging library.
    /// </summary>
    public static IServiceCollection AddLoggingLibrary(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName,
        Action<LoggingOptions>? configure = null)
    {
        // Create options with defaults
        var options = new LoggingOptions
        {
            ApplicationName = applicationName,
            EnableApiLogging = true,
            EnableExceptionLogging = true,
            EnablePerformanceLogging = true,
            EnableQueryLogging = true,
            SlowQueryThresholdMs = 500,
            BatchSize = 100,
            BatchIntervalMs = 1000,
            MaxQueueSize = 10000,
            EnableTracing = true,
            EnableMetrics = true,
            OtelServiceName = applicationName
        };

        // Apply custom configuration
        configure?.Invoke(options);

        // Configure options
        services.Configure<LoggingOptions>(opt =>
        {
            opt.ApplicationName = applicationName;
            opt.EnableApiLogging = options.EnableApiLogging;
            opt.EnableExceptionLogging = options.EnableExceptionLogging;
            opt.EnablePerformanceLogging = options.EnablePerformanceLogging;
            opt.EnableQueryLogging = options.EnableQueryLogging;
            opt.SlowQueryThresholdMs = options.SlowQueryThresholdMs;
            opt.BatchSize = options.BatchSize;
            opt.BatchIntervalMs = options.BatchIntervalMs;
            opt.MaxQueueSize = options.MaxQueueSize;
            opt.EnableTracing = options.EnableTracing;
            opt.EnableMetrics = options.EnableMetrics;
            opt.OtelServiceName = options.OtelServiceName;
        });

        // Add Serilog options from configuration
        services.Configure<SerilogOptions>(configuration.GetSection("Serilog"));
        services.Configure<OpenTelemetryOptions>(configuration.GetSection("OpenTelemetry"));

        // Core services
        services.AddHttpContextAccessor();
        services.AddScoped<ITraceIdAccessor, TraceIdAccessor>();

        // Add logging services
        services.AddLoggingServices(configure);

        // Add OpenTelemetry
        services.AddOpenTelemetryLogging(configuration, opt =>
        {
            opt.ServiceName = options.OtelServiceName;
            opt.EnableTracing = options.EnableTracing;
            opt.EnableMetrics = options.EnableMetrics;
        });

        return services;
    }

    /// <summary>
    /// Adds the complete logging library with a connection string for database logging.
    /// </summary>
    public static IServiceCollection AddLoggingLibrary(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName,
        string loggingConnectionString,
        Action<LoggingOptions>? configure = null)
    {
        // Add database context first
        services.AddLoggingDbContext(loggingConnectionString);

        // Add the rest of the logging library
        return services.AddLoggingLibrary(configuration, applicationName, configure);
    }

    #endregion

    #region Individual Service Add Methods

    /// <summary>
    /// Adds logging options from configuration.
    /// </summary>
    public static IServiceCollection AddLoggingOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LoggingOptions>(
            configuration.GetSection("LoggingSettings"));

        return services;
    }

    /// <summary>
    /// Adds Serilog to the host.
    /// </summary>
    public static IHostBuilder AddSerilogLogging(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string applicationName)
    {
        return hostBuilder.ConfigureSerilog(configuration, applicationName);
    }

    #endregion

    #region Serilog Configuration

    /// <summary>
    /// Configures Serilog for the application.
    /// </summary>
    public static IHostBuilder ConfigureSerilog(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string applicationName,
        Action<SerilogOptions>? configure = null,
        LogEventLevel minimumLevel = LogEventLevel.Information)

    {

        return hostBuilder.UseSerilog((context, services, loggerConfig) =>
        {
            // Read from configuration
            loggerConfig
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", applicationName)
                .MinimumLevel.Is(minimumLevel);

            // Apply custom configuration
            var options = new SerilogOptions();
            configure?.Invoke(options);

            // Apply additional enrichers from options
            foreach (var prop in options.Properties)
            {
                loggerConfig.Enrich.WithProperty(prop.Key, prop.Value);
            }
        });
    }

    /// <summary>
    /// Initializes Serilog logger from configuration.
    /// </summary>
    public static LoggerConfiguration InitializeSerilog(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        string applicationName)
    {
        return loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName);
    }

    #endregion

    #region OpenTelemetry Configuration

    /// <summary>
    /// Adds OpenTelemetry tracing and metrics services.
    /// </summary>
    public static IServiceCollection AddOpenTelemetryLogging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<OpenTelemetryOptions>? configureOptions = null)
    {
        var options = new OpenTelemetryOptions();
        configureOptions?.Invoke(options);

        // Bind from configuration if section exists
        configuration.GetSection("OpenTelemetry").Bind(options);

        // Register TelemetryActivitySource as singleton
        services.AddSingleton(TelemetryActivitySource.Instance);

        // Add OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName: options.ServiceName))
            .WithTracing(tracing =>
            {
                if (options.EnableTracing)
                {
                    tracing
                        .AddSource(TelemetryActivitySource.Instance.Name)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                    // Add OTLP exporter if endpoint is configured
                    if (!string.IsNullOrEmpty(options.OtlpEndpoint))
                    {
                        tracing.AddOtlpExporter(otlp =>
                        {
                            otlp.Endpoint = new Uri(options.OtlpEndpoint);
                        });
                    }
                    else
                    {
                        tracing.AddOtlpExporter();
                    }
                }
            })
            .WithMetrics(metrics =>
            {
                if (options.EnableMetrics)
                {
                    metrics
                        .AddRuntimeInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                    if (!string.IsNullOrEmpty(options.OtlpEndpoint))
                    {
                        metrics.AddOtlpExporter(otlp =>
                        {
                            otlp.Endpoint = new Uri(options.OtlpEndpoint);
                        });
                    }
                    else
                    {
                        metrics.AddOtlpExporter();
                    }
                }
            });

        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry with default configuration.
    /// </summary>
    public static IServiceCollection AddOpenTelemetryLogging(
        this IServiceCollection services,
        string serviceName)
    {
        return services.AddOpenTelemetryLogging(
            configuration: null!,
            options =>
            {
                options.ServiceName = serviceName;
                options.EnableTracing = true;
                options.EnableMetrics = true;
            });
    }

    #endregion

    #region Middleware Configuration

    /// <summary>
    /// Uses all logging library middleware in the correct order.
    /// </summary>
    public static IApplicationBuilder UseLoggingLibrary(this IApplicationBuilder app)
    {
        // Exception handling middleware (first in pipeline)
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // OpenTelemetry logging middleware
        app.UseMiddleware<OpenTelemetryLoggingMiddleware>();

        // Serilog request logging
        app.UseSerilogRequestLogging();

        return app;
    }

    /// <summary>
    /// Uses the exception handling middleware.
    /// </summary>
    public static IApplicationBuilder UseLoggingExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    /// <summary>
    /// Uses the OpenTelemetry logging middleware.
    /// </summary>
    public static IApplicationBuilder UseOpenTelemetryLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<OpenTelemetryLoggingMiddleware>();
    }
     

    #endregion
}
