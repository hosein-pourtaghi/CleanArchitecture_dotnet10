using Web.Api.Infrastructure;
using Web.Api.Telemetry;

namespace Web.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // REMARK: If you want to use Controllers, you'll need this.
        services.AddControllers();

        // Register the new OpenTelemetry-enabled exception handler
        services.AddExceptionHandler<OpenTelemetryExceptionHandler>();
        services.AddProblemDetails();

        // Register ActivitySource for dependency injection if needed
        services.AddSingleton(TelemetryActivitySource.Instance);

        return services;
    }
}
