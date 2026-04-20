// src/LoggingLibrary/Extensions/ServiceCollectionExtensions.cs

using SharedKernel.LoggingCore.Configuration;
using SharedKernel.LoggingCore.DependencyInjection;
using SharedKernel.LoggingCore.Services;
using LoggingLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.LoggingCore.Services;

namespace SharedKernel.LoggingCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoggingLibrary(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<LoggingOptions>? configure = null)
    {
        // Configure options
        services.Configure(configure ?? (_ => { }));
        services.AddLoggingOptions(configuration);
        // Core services
        services.AddHttpContextAccessor();
        services.AddScoped<ITraceIdAccessor, TraceIdAccessor>();

        services.AddLoggingServices(options =>
        {
            options.EnableApiLogging = true;
            options.EnableExceptionLogging = true;
            options.EnablePerformanceLogging = true;
            options.EnableQueryLogging = true;  // Only in dev
            options.SlowQueryThresholdMs = 500;  // Log queries > 500ms
            options.ShowDetailsInProduction = true;
            options.BatchSize = 100;
            options.BatchIntervalMs = 1000;
            options.MaxQueueSize = 10000;
        });



        return services;
    }

    public static IApplicationBuilder UseLoggingLibrary(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
