// src/LoggingLibrary/Extensions/ServiceCollectionExtensions.cs

using LoggingCore.Configuration;
using LoggingCore.Services;
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

        return services;
    }

    public static IApplicationBuilder UseLoggingLibrary(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
