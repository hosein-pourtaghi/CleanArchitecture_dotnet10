// src/WebApi/DependencyInjection.cs
// ============================================================
// PURPOSE: Presentation layer dependency injection configuration
// ============================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using SharedApi.DynamicCrud;

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
            ConfigureSwaggerDocument(options);
            ConfigureSchemaIds(options);
            ConfigureJwtAuthentication(options);
            ConfigureXmlComments(options);
            ConfigureOrdering(options);
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
        .AddControllersAsServices() 
        ;


        services.AddDynamicCrud();

        // Custom model binders (commented out - uncomment if needed)
        // services.AddSingleton<IModelBinderProvider, GenericFilterModelBinderProvider>();

        // Problem Details for standardized error responses
        services.AddProblemDetails();


        return services;
    }





    #region Swagger functions

    private static void ConfigureSwaggerDocument(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Clean Architecture API",
            Version = "v1",
            Description =
                "Enterprise-grade ASP.NET Core Web API built with Clean Architecture",
            Contact = new OpenApiContact
            {
                Name = "Development Team"
            }
        });
    }


    private static void ConfigureSchemaIds(SwaggerGenOptions options)
    {
        // Prevent duplicate schema names:
        // Example:
        // User.Application.DTO.UserDto
        // User.Domain.Entities.User

        options.CustomSchemaIds(type =>
        {
            return type.FullName?
                .Replace("+", ".")
                .Replace("`1", "")
                ??
                type.Name;
        });
    }

    private static void ConfigureJwtAuthentication(
    SwaggerGenOptions options)
    {
        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    """
                JWT Authorization header.

                Enter:
                Bearer {your_token}
                """
            });


        options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
          {
              new OpenApiSecuritySchemeReference(
                  "Bearer",
                  document
              ),
              new List<string>()
          }
        });

    }

    private static void ConfigureXmlComments(
        SwaggerGenOptions options)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var xmlFile =
            $"{assembly.GetName().Name}.xml";

        var xmlPath =
            Path.Combine(
                AppContext.BaseDirectory,
                xmlFile);


        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    }


    private static void ConfigureOrdering(
        SwaggerGenOptions options)
    {
        options.DocInclusionPredicate(
            (_, _) => true);

        options.OrderActionsBy(api =>
            api.RelativePath ?? string.Empty);
    }
    #endregion



}

// Helper method to get assembly reference
file static class Assembly
{
    public static System.Reflection.Assembly GetExecutingAssembly() =>
        System.Reflection.Assembly.GetExecutingAssembly();
}
