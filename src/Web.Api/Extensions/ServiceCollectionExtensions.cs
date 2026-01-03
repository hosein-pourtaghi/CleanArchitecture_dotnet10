using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            // Add API documentation
            options.SwaggerDoc("v1", new()
            {
                Title = "CleanArchitecture API",
                Version = "v1.0",
                Description = "Comprehensive REST API for Customer Management System with JWT Authentication. " +
                              "This API provides secure endpoints for managing customers with role-based access control.",
                Contact = new()
                {
                    Name = "API Support",
                    Email = "support@cleanarchitecture.local",
                    Url = new Uri("https://github.com/yourusername/CleanArchitecture_dotnet10")
                },
                License = new()
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                },
                TermsOfService = new Uri("https://example.com/terms")
            });

            // Include XML documentation comments
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Document all endpoints
            options.DocInclusionPredicate((_, _) => true);

            // Sort endpoints by route
            options.OrderActionsBy(api => api.RelativePath);
        });

        return services;
    }
}

