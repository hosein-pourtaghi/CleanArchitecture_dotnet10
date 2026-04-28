// src/Infrastructure/DependencyInjection.cs
// ============================================================
// PURPOSE: Infrastructure layer dependency injection configuration
// ============================================================

using Application.Common.Interfaces.Checklists;
using Application.Common.Interfaces.Core;
//using Infrastructure.Authorization;
using Infrastructure.DomainEvents;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Core;
//using Infrastructure.Services.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SharedKernel.LoggingCore.Interceptors;
using SharedKernel.Shared;

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
            ;

    /// <summary>
    /// Adds general infrastructure services.
    /// </summary>
    private static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        // HTTP context accessor for getting current user info
        services.AddHttpContextAccessor();

        // Memory cache for performance optimization
        services.AddMemoryCache();

        // Base repository implementation
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

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
     

}
