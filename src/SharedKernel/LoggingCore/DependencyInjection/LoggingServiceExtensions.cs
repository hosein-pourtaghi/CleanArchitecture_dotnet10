// src/LoggingCore/DependencyInjection/LoggingServiceExtensions.cs
using LoggingCore.Configuration;
using LoggingCore.Data;
using LoggingCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LoggingCore.DependencyInjection;

public static class LoggingServiceExtensions
{
    /// <summary>
    /// Add logging services to the service collection
    /// </summary>
    public static IServiceCollection AddLoggingServices(
        this IServiceCollection services,
        Action<LoggingOptions>? configureOptions = null)
    {
        // Configure options
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
                    options.BatchSize = 100;
                    options.BatchIntervalMs = 1000;
                    options.MaxQueueSize = 10000;
                });
        }

        // Register logging service
        services.AddSingleton<ILoggingService, AsyncLoggingService>();
        // Register as hosted service
        services.AddHostedService(sp => (AsyncLoggingService)sp.GetRequiredService<ILoggingService>());

        return services;
    }

    /// <summary>
    /// Add logging database context
    /// </summary>
    public static IServiceCollection AddLoggingDbContext(
       this IServiceCollection services,
       string connectionString,
       bool useInMemory = false)
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

        //services.AddScoped<LoggingDbContext>();

        return services;
    }

    /// <summary>
    /// Ensure logging database is created
    /// </summary>
    public static void InitializeLoggingDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();
        dbContext.Database.EnsureCreated();
    }
}
