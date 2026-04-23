// src/Infrastructure/DependencyInjection.cs
// ============================================================
// PURPOSE: Infrastructure layer dependency injection configuration
// ============================================================

using System.Security.Claims;
using System.Text;
using Application.Common.Interfaces.Checklists;
using Application.Common.Interfaces.Core;
using Domain.Aggregates.Identities;
using Infrastructure.Authorization;
using Infrastructure.DomainEvents;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Core;
using Infrastructure.Services;
using Infrastructure.Services.Identities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SharedKernel.LoggingCore.Interceptors;

namespace Infrastructure;

/// <summary>
/// Extension methods for configuring infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices(configuration)
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddAuthentication(configuration)
            .AddAuthorization()
            .AddPolicyDiscovery()
            ;

    /// <summary>
    /// Adds general infrastructure services.
    /// </summary>
    private static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Identity services
        services.AddScoped<IIdentitySeeder, IdentitySeeder>();

        // HTTP context accessor for getting current user info
        services.AddHttpContextAccessor();

        // Memory cache for performance optimization
        services.AddMemoryCache();

        // Base repository implementation
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        // Current user service for accessing authenticated user info
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Advanced repository (commented out - uncomment if needed)
        #region Advanced Repository Injection
        // services.AddScoped(typeof(IAdvancedRepository<>), typeof(AdvancedRepository<>));
        // services.Scan(scan => scan
        //     .FromAssembliesOf(typeof(ApplicationDbContext))
        //     .AddClasses(classes => classes.AssignableTo(typeof(IAdvancedRepository<,>)))
        //     .AsImplementedInterfaces()
        //     .WithScopedLifetime());
        #endregion

        // Checklist repository
        services.AddScoped<IChecklistRepository, ChecklistRepository>();

        // Domain events dispatcher
        services.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();


        return services;
    }

    /// <summary>
    /// Adds database context and related services.
    /// </summary>
    private static IServiceCollection AddDatabase(
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
        services.AddDbContext<ApplicationDbContext>(
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
                options.AddInterceptors(
                    serviceProvider.GetRequiredService<QueryLoggingInterceptor>());

                // Development-only options (remove in production)
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

        // Register DbContext as IApplicationDbContext for cleaner DI
        services.AddScoped<IApplicationDbContext>(
            sp => sp.GetRequiredService<ApplicationDbContext>());

        // Register query logging interceptor
        services.AddScoped<QueryLoggingInterceptor>();

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
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddRoleManager<RoleManager<ApplicationRole>>();

        return services;
    }

    /// <summary>
    /// Adds health check services.
    /// </summary>
    private static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get the appropriate connection string based on database type
        string? connectionString = configuration["ConnectionString"];

        services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"])
            // Use SQL Server health check (matches connection string)
            .AddSqlServer(
                connectionString!,
                name: "sqlserver",
                tags: new[] { "db", "sql", "sqlserver" }
                )
            ;


        // Alternative: PostgreSQL health check (uncomment if using PostgreSQL)
        // .AddNpgSql(configuration["ConnectionString"]!);

        return services;
    }

    /// <summary>
    /// Adds authentication services.
    /// </summary>
    private static IServiceCollection AddAuthentication(
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
            o.RequireHttpsMetadata = false; // Set to true in production
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
                #region Token Validation (if custom middleware not enabled)
                // OnTokenValidated = async context =>
                // {
                //     var services = context.HttpContext.RequestServices;
                //     
                //     // 1. Check if token is blacklisted
                //     var tokenId = context.Principal?.FindFirst("jti")?.Value;
                //     if (!string.IsNullOrEmpty(tokenId))
                //     {
                //         using var scope = services.CreateScope();
                //         var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //         var isBlacklisted = await dbContext.TokenBlacklist
                //             .AnyAsync(t => t.TokenId == tokenId && t.ExpiresAt > DateTime.UtcNow);
                //         if (isBlacklisted)
                //         {
                //             context.Fail("Token has been revoked");
                //             return;
                //         }
                //     }
                //     
                //     // 2. Check token version
                //     var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                //     var tokenVersionClaim = context.Principal?.FindFirst("token_version")?.Value;
                //     if (!string.IsNullOrEmpty(userIdClaim) && !string.IsNullOrEmpty(tokenVersionClaim))
                //     {
                //         using var scope = services.CreateScope();
                //         var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //         var user = await dbContext.Users.FindAsync(Guid.Parse(userIdClaim));
                //         if (user != null && user.TokenVersion != int.Parse(tokenVersionClaim))
                //         {
                //             context.Fail("Token has been revoked");
                //             return;
                //         }
                //     }
                // },
                #endregion

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
    private static IServiceCollection AddAuthorization(this IServiceCollection services)
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


}
