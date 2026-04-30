// src/WebApi/DependencyInjection.cs
// ============================================================
// PURPOSE: Presentation layer dependency injection configuration
// ============================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace WebApi;

/// <summary>
/// Extension methods for configuring presentation layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds presentation layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        // ============================================================
        // STEP 2: Configure CORS Policy
        // ============================================================
        // NOTE: If using credentials (cookies/auth), use WithOrigins() instead of AllowAnyOrigin()
        // and remove AllowAnyOrigin(). AllowAnyOrigin() + AllowCredentials() throws at runtime.
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularApp", policy =>
            {
                // For development - allows any origin
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();

                // For production with specific origins:
                // policy.WithOrigins("https://yourdomain.com")
                //       .AllowAnyHeader()
                //       .AllowAnyMethod();

                // For credentials-based auth (uncomment if needed):
                // policy.AllowCredentials(); // Cannot be used with AllowAnyOrigin()
            });
        });

        // ============================================================
        // Swagger Configuration
        // ============================================================
        services.AddSwaggerGen(options =>
        {
            // Use full type name for schema IDs to avoid conflicts
            options.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "WebApi",
                Description = "My Project API",
                // TermsOfService = new Uri("https://example.com/terms"),
                // Contact = new OpenApiContact
                // {
                //     Name = "Development Team",
                //     Url = new Uri("https://example.com")
                // }
            });

            // Bearer Token Security Scheme
            // Allows entering JWT token in Swagger UI
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token in the text field below.\n\n" +
                              "Example: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                Name = "Authorization",
                In = ParameterLocation.Header
            });

            // Apply Bearer security globally to all endpoints
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML documentation comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
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

        // ============================================================
        // Controllers Configuration
        // ============================================================
        services.AddControllers(options =>
        {
            // Global response type filter
            options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status200OK));

            // Add more global filters here as needed
            // options.Filters.Add(new AuthorizeFilter());
        })
        .AddJsonOptions(options =>
        {
            // Serialize enums as strings for better readability
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
             
            // Convert property names to camelCase
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            // Also convert dictionary keys
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

            options.JsonSerializerOptions.WriteIndented = false;

        })
        .AddControllersAsServices();

        // Custom model binders (commented out - uncomment if needed)
        // services.AddSingleton<IModelBinderProvider, GenericFilterModelBinderProvider>();

        // Problem Details for standardized error responses
        services.AddProblemDetails();


        return services;
    }

}

// Helper method to get assembly reference
file static class Assembly
{
    public static System.Reflection.Assembly GetExecutingAssembly() =>
        System.Reflection.Assembly.GetExecutingAssembly();
}
