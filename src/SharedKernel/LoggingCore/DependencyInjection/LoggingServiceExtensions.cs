// src/LoggingCore/DependencyInjection/LoggingServiceExtensions.cs
// ============================================================
// PURPOSE: Extension methods for registering logging services
// ============================================================

using SharedKernel.LoggingCore.Configuration;
using SharedKernel.LoggingCore.Data;
using SharedKernel.LoggingCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SharedKernel.LoggingCore.DependencyInjection;

/// <summary>
/// Extension methods for configuring logging services in the DI container.
/// </summary>
public static class LoggingServiceExtensions
{
    /// <summary>
    /// Adds logging services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure logging options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoggingServices(
        this IServiceCollection services,
        Action<LoggingOptions>? configureOptions = null)
    {
        // Configure options using Options pattern
        if (configureOptions != null)
        {
            // If custom configuration provided, use it directly
            services.Configure(configureOptions);
        }
        else
        {
            // Otherwise, use default configuration
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
        // IHostedService is auto-discovered by the runtime
        services.AddHostedService(sp => (LoggingService)sp.GetRequiredService<ILoggingService>());

        // Alternative registration (commented out - use one or the other):
        // services.AddSingleton<LoggingService>();
        // services.AddSingleton<ILoggingService>(sp => sp.GetRequiredService<LoggingService>());

        return services;
    }

    /// <summary>
    /// Adds the logging database context.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="useInMemory">Whether to use in-memory database (for testing).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoggingDbContext(
        this IServiceCollection services,
        string connectionString,
        bool useInMemory = false)
    {
        if (useInMemory)
        {
            //// Use in-memory database for testing scenarios
            //services.AddDbContext<LoggingDbContext>(options =>
            //    options.UseInMemoryDatabase("LoggingDb"));
        }
        else
        {
            // Use SQL Server for production
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

        // Alternative: Register as scoped explicitly (usually not needed as AddDbContext handles this)
        // services.AddScoped<LoggingDbContext>();

        return services;
    }

    /// <summary>
    /// Ensures the logging database is created.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task InitializeLoggingDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();

        // Ensure database and schema exist
        await dbContext.Database.EnsureCreatedAsync();
    }
}
