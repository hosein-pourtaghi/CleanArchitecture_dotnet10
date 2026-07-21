using System.Security.Claims;
using System.Text;
using IdentityApi.Application.Interfaces;
using IdentityApi.Domain.Entities;
using IdentityApi.Infrastructure.Authorization;
using IdentityApi.Infrastructure.Persistence;
using IdentityApi.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SharedKernel.LoggingCore.DependencyInjection; 
using SharedKernel.Shared;

namespace Infrastructure;

/// <summary>
/// Extension methods for configuring infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all identity infrastructure services.
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLoggingServices(configuration);
        services.AddIdentityServices(configuration);
        services.AddIdentityDatabase(configuration);
        services.AddMyIdentityAuthentication(configuration);
        services.AddMyIdentityAuthorization();
        services.AddPolicyDiscovery();

        return services;
    }

    private static void AddLoggingServices(this IServiceCollection services, IConfiguration configuration)
    {


        // ============================================================
        // STEP 1: Configure Logging Library (Serilog + OpenTelemetry + Services)
        // ============================================================
        string? loggingConnectionString = configuration.GetConnectionString("LoggingConnection");
        if (string.IsNullOrEmpty(loggingConnectionString))
        {
            throw new InvalidOperationException(
                "LoggingConnection string is not configured. " +
                "Add 'ConnectionStrings:LoggingConnection' to your configuration.");
        }

        // Single call to add everything: Serilog, OpenTelemetry, Logging Services, DbContext
        services.AddLoggingLibrary(
            configuration,
            "IdentityApi", // Application name for this library
            loggingConnectionString,
            options =>
            {
                options.EnableApiLogging = true;
                options.EnableExceptionLogging = true;
                options.EnablePerformanceLogging = true;
                options.EnableQueryLogging = true;
                options.SlowQueryThresholdMs = 500;
            });
    }


    /// <summary>
    /// Adds database context and related services.
    /// </summary>
    private static IServiceCollection AddIdentityDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration["ConnectionString"];

        // Alternative: PostgreSQL configuration (commented out)
        #region PostgreSQL Configuration
        // services.AddDbContext<ApplicationDbContext>(options => options
        //     .UseNpgsql(connectionString, npgsqlOptions =>
        //         npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
        //     );
        #endregion

        // SQL Server configuration
        services.AddDbContext<MyIdentityDbContext>(
            (serviceProvider, options) =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        Schemas.Default);

                    // Command timeout of 3 minutes for long-running operations
                    sqlOptions.CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds);

                    // Enable retry on transient failures
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);

                    // Batch size optimization for bulk operations
                    sqlOptions.MinBatchSize(10);
                    sqlOptions.MaxBatchSize(100);
                });

                // Add query logging interceptor
                //options.AddInterceptors(serviceProvider.GetRequiredService<QueryLoggingInterceptor>());

                // Development-only options (remove in production)
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

        // Register DbContext as IApplicationDbContext for cleaner DI
        services.AddScoped<IMyIdentityDbContext>(
            sp => sp.GetRequiredService<MyIdentityDbContext>());

        //// Register query logging interceptor
        //services.AddScoped<QueryLoggingInterceptor>();

        // Configure ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // User settings
            options.User.RequireUniqueEmail = true;

            // Password settings (OWASP compliant)
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
        })
            .AddEntityFrameworkStores<MyIdentityDbContext>()
            .AddDefaultTokenProviders()
            .AddRoleManager<RoleManager<ApplicationRole>>();

        return services;
    }

    private static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Identity services
        services.AddScoped<IIdentitySeeder, IdentitySeeder>();

        // Memory cache for performance optimization
        services.AddMemoryCache();

        // Current user service for accessing authenticated user info
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
     
    /// <summary>
    /// Adds authentication services.
    /// </summary>
    private static IServiceCollection AddMyIdentityAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = true; // Set to true in production
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role
            };

            o.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    if (ctx.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        ctx.Response.Headers.Add("Token-Expired", "true");
                    }

                    Log.Error(ctx.Exception, "JWT Authentication Failed: {Message}",
                        ctx.Exception.Message);

                    return Task.CompletedTask;
                },

                OnChallenge = ctx =>
                {
                    Log.Warning("JWT Challenge issued: {Error} - {ErrorDescription}",
                        ctx.Error,
                        ctx.ErrorDescription);

                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    /// <summary>
    /// Adds authorization services with permission-based access control.
    /// </summary>
    private static IServiceCollection AddMyIdentityAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Default policy - requires authenticated user
            // This makes [Authorize] attribute work by default
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement("__default__"))
                .Build();

            // Additional policies are auto-discovered by PolicyDiscoveryService on startup
        });

        // Auth & Token Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();

        // Authorization infrastructure
        services.AddScoped<PermissionProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
    private static IServiceCollection AddPolicyDiscovery(this IServiceCollection services)
    {
        // Policy Discovery Service - scans controllers for authorization policies
        // Must be registered AFTER AddDatabase() and AddAuthorization() 
        // because it depends on ApplicationDbContext and AuthorizationOptions
        services.AddScoped<IPolicyDiscoveryService, PolicyDiscoveryService>();

        return services;
    }
     
    /// <summary>
    /// Discover and register authorization policies
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static async Task RegisterAuthorizationPolicies(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var policyService = scope.ServiceProvider.GetRequiredService<IPolicyDiscoveryService>();
            var result = await policyService.DiscoverAndRegisterPoliciesAsync();

            if (result.IsSuccess && result.Value > 0)
            {
                Log.Information("Registered {PolicyCount} authorization policies", result.Value);
            }
            else if (result.IsFailure)
            {
                Log.Warning("Policy discovery failed: {Error}", result.Error.Description);
            }
        }
    }

    public static async Task InitializeDatabaseSeedData(this WebApplication app, WebApplicationBuilder builder)
    {
        // ============================================
        // Initialize Database & Seed Data
        // ============================================
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                // Apply migrations
                var dbContext = services.GetRequiredService<MyIdentityDbContext>();
                await dbContext.Database.MigrateAsync();

                Log.Information("Database initialized successfully");

                // Seed identity data
                var identitySeeder = scope.ServiceProvider.GetRequiredService<IIdentitySeeder>();

                var adminEmail = builder.Configuration["Admin:Email"] ?? "admin@example.com";
                var adminPassword = builder.Configuration["Admin:Password"] ?? "Admin@123456";
                var adminRole = builder.Configuration["Admin:Role"] ?? "Admin";

                await identitySeeder.SeedAdminUserAsync(adminEmail, adminPassword, adminRole);
                Log.Information("Admin user seeding completed for {Email}", adminEmail);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while initializing the database");
            }
        }
    }




}
