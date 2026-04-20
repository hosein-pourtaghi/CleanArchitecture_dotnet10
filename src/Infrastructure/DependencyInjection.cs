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
using SharedKernel.LoggingCore.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using static System.Formats.Asn1.AsnWriter;

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
        services.AddScoped<IIdentitySeeder, IdentitySeeder>();

        services.AddHttpContextAccessor();
        services.AddMemoryCache();

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


        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {

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

                options.AddInterceptors(serviceProvider.GetRequiredService<QueryLoggingInterceptor>());

                // فقط برای محیط توسعه!
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();

            });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());



        // Todo: Register interceptor do it in Library
        services.AddScoped<QueryLoggingInterceptor>();




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
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };

                o.Events = new JwtBearerEvents
                {
                    #region Validate token on every request if Custom middleware not Enabled
                    //OnTokenValidated = async context =>
                    //{
                    //    var services = context.HttpContext.RequestServices;

                    //    // 1. Check if token is blacklisted
                    //    var tokenId = context.Principal?.FindFirst("jti")?.Value;
                    //    if (!string.IsNullOrEmpty(tokenId))
                    //    {
                    //        using var scope = services.CreateScope();
                    //        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    //        var isBlacklisted = await dbContext.TokenBlacklist
                    //            .AnyAsync(t => t.TokenId == tokenId && t.ExpiresAt > DateTime.UtcNow);

                    //        if (isBlacklisted)
                    //        {
                    //            context.Fail("Token has been revoked");
                    //            return;
                    //        }
                    //    }

                    //    // 2. Check token version
                    //    var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    //    var tokenVersionClaim = context.Principal?.FindFirst("token_version")?.Value;

                    //    if (!string.IsNullOrEmpty(userIdClaim) && !string.IsNullOrEmpty(tokenVersionClaim))
                    //    {
                    //        using var scope = services.CreateScope();
                    //        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    //        var user = await dbContext.Users.FindAsync(Guid.Parse(userIdClaim));
                    //        if (user != null && user.TokenVersion != int.Parse(tokenVersionClaim))
                    //        {
                    //            context.Fail("Token has been revoked");
                    //            return;
                    //        }
                    //    }
                    //},
                    #endregion

                    OnAuthenticationFailed = ctx =>
                    {
                        if (ctx.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            ctx.Response.Headers.Add("Token-Expired", "true");
                        }
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

        return services;
    }

    private static IServiceCollection AddAuthorization(this IServiceCollection services)
    {
        // ====================
        // Authorization
        // ====================
        services.AddAuthorization(options =>
        {
            // ✅ ADD THIS - Makes [Authorize] check permissions
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement("__default__"))
                .Build();

            // Your existing policies (auto-discovered)
            // These are added by PolicyDiscoveryService on startup
        });
        // Auth & Token Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();

        // Authorization

        services.AddScoped<PermissionProvider>();
        // Evaluates authorization logic
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        // Provides / creates policies dynamically
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        // Policy discovery service
        //services.AddScoped<IPolicyDiscoveryService, PolicyDiscoveryService>();

        return services;
    }


}
