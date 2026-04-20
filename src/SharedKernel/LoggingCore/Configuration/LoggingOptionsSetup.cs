// src/LoggingCore/Configuration/LoggingOptionsSetup.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.LoggingCore.Configuration;

public static class LoggingOptionsSetup
{
    public static IServiceCollection AddLoggingOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LoggingOptions>(
            configuration.GetSection(LoggingOptions.SectionName),
            binderOptions => binderOptions.BindNonPublicProperties = true);

        return services;
    }
}
