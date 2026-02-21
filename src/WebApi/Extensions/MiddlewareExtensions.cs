using WebApi.Middleware;

namespace WebApi.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    { 
        // Custom Body Logging
        app.UseMiddleware<RequestResponseLoggingMiddleware>();

        return app;
    }
}
