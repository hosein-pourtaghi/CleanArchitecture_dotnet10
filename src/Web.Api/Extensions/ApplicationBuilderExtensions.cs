namespace Web.Api.Extensions;

/// <summary>
/// Extension methods for configuring middleware in the application pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Swagger and Swagger UI to the application pipeline.
    /// Configures Swagger UI for interactive API exploration with authentication support.
    /// </summary>
    /// <param name="app">The application builder instance.</param>
    /// <returns>The application builder instance for chaining.</returns>
    public static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
    {
        // Enable Swagger middleware
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        // Enable Swagger UI
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "CleanArchitecture API v1");
            options.RoutePrefix = "swagger";
            
            // Swagger UI configuration
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.DefaultModelsExpandDepth(1);
            options.DefaultModelExpandDepth(1);
            options.DisplayOperationId();
            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();
            options.EnableFilter();
        });

        return app;
    }
}
