using System;
using System.Security.Claims;
using System.Text;
using Application.Common.Authentication;
using Application.Common.Data;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Checklists;
using Domain.Entities.Identities;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.DomainEvents;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices(configuration)
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorization();

    private static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        #region Advanced Repository Injection  
        //services.AddScoped(typeof(IAdvancedRepository<>), typeof(AdvancedRepository<>));

        //services.Scan(scan => scan
        //    .FromAssembliesOf(typeof(ApplicationDbContext))
        //    .AddClasses(classes => classes.AssignableTo(typeof(IAdvancedRepository<,>)))
        //    .AsImplementedInterfaces()
        //    .WithScopedLifetime());
        #endregion

        services.AddScoped<IChecklistRepository, ChecklistRepository>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        // Auth & Token Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();

        // Authorization
        services.AddSingleton<PermissionProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Register diagnostic service FIRST (before DbContext)
        //services.AddSingleton<IQueryDiagnosticsService, QueryDiagnosticsService>();

        string? connectionString = configuration["ConnectionString"];

        //services.AddDbContext<ApplicationDbContext>(options => options
        //    .UseNpgsql(connectionString, npgsqlOptions =>
        //        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
        //    );

        services.AddDbContext<ApplicationDbContext>(
            (serviceProvider, options) =>
            //options =>
            {
                options.UseSqlServer(connectionString, options =>
                {
                    options.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default);
                    var seconds = (int)TimeSpan.FromMinutes(3).TotalSeconds;
                    options.CommandTimeout(seconds);
                    options.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);

                    // 🔥 BULK OPERATIONS: Minimize logging overhead
                    options.MinBatchSize(10);
                    options.MaxBatchSize(100);
                });

                //// Add interceptors
                //var diagnosticsService = serviceProvider.GetRequiredService<IQueryDiagnosticsService>();
                //var logger = serviceProvider.GetRequiredService<ILogger<QueryLoggingInterceptor>>();

                //options.AddInterceptors(new QueryLoggingInterceptor(diagnosticsService, logger));

                // فقط برای محیط توسعه!
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();

            });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddRoleManager<RoleManager<ApplicationRole>>()
            ;
        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration["ConnectionString"]!);

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
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
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {

                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,


                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = JwtRegisteredClaimNames.Sub
                };

                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine("JWT FAILED:");
                        Console.WriteLine(ctx.Exception.ToString());
                        Log.Error("JWT FAILED:");
                        Log.Error("JWT FAILED Exeption:", ctx.Exception.ToString());
                        return Task.CompletedTask;
                    },
                    OnChallenge = ctx =>
                    {
                        Console.WriteLine("JWT CHALLENGE");
                        Log.Error("JWT CHALLENGE");
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddAuthorization(this IServiceCollection services)
    {
        //PolicyServiceCollectionExtensions.AddAuthorization(services);

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Permission:users.read", policy =>
                policy.Requirements.Add(new PermissionRequirement("users.read")));

            options.AddPolicy("Permission:users.create", policy =>
                policy.Requirements.Add(new PermissionRequirement("users.create")));

            // Add more policies as needed
        });

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }



}
