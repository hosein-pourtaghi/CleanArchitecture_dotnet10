using Application;
using Domain.Entities.Identities;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Authorization;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using WebApi;
using WebApi.Extensions;
using WebApi.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder =>
        {
            builder.AllowAnyOrigin()
            //.WithOrigins("http://localhost:4200") // Your Angular app URL
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   //.AllowCredentials() // If you're using cookies/auth
                   ;
        });
});


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MyAspireApp")
    .CreateLogger();


builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("WebApi")
            //.AddConsoleExporter()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    })
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder
            .AddRuntimeInstrumentation()
            //.AddConsoleExporter()
            .AddOtlpExporter();
    });
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

//// Configure Serilog for SQL logging
//builder.Host.UseSerilog((context, services, configuration) =>
//    configuration
//        .Enrich.FromLogContext()
//        .Enrich.WithEnvironmentName()
//        .Enrich.WithMachineName()
//        .WriteTo.Console()
//        .WriteTo.MSSqlServer(
//            connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
//            tableName: "RequestLogs",
//            autoCreateSqlTable: true,
//            columnOptions: new ColumnOptions
//            {
//                AdditionalColumns = new List<SqlColumn>
//                {
//                    new SqlColumn { ColumnName = "CorrelationId", DataType = SqlDbType.NVarChar, DataLength = 50 },
//                    new SqlColumn { ColumnName = "HttpMethod", DataType = SqlDbType.NVarChar, DataLength = 10 },
//                    new SqlColumn { ColumnName = "HttpPath", DataType = SqlDbType.NVarChar, DataLength = 255 },
//                    new SqlColumn { ColumnName = "HttpStatus", DataType = SqlDbType.Int },
//                    new SqlColumn { ColumnName = "ResponseTimeMs", DataType = SqlDbType.Int }
//                }
//            },
//            sinkOptions: new MSSqlServerSinkOptions
//            {
//                BatchPostingLimit = 100,
//                BatchPeriod = TimeSpan.FromSeconds(5)
//            }
//        )
//);


//builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IPolicyDiscoveryService, PolicyDiscoveryService>();

WebApplication app; 
try
{
      app = builder.Build();

}
catch (Exception e)
{

    throw;
}


// Discover and register policies on startup
using (var scope = app.Services.CreateScope())
{
    var policyService = scope.ServiceProvider.GetRequiredService<IPolicyDiscoveryService>();
    var result = await policyService.DiscoverAndRegisterPoliciesAsync();

    if (result.IsSuccess && result.Value > 0)
    {
        Console.WriteLine($"Registered {result.Value} authorization policies");
    }
}

// just for test
//app.Use(async (context, next) => 
//{
//    var authorizationHeader = context.Request.Headers["Authorization"];
//    if (!string.IsNullOrEmpty(authorizationHeader))
//    {
//        // Log the token for debugging purposes
//        Console.WriteLine("Authorization Header: " + authorizationHeader);
//    }
//    await next();
//});


// Use CORS (cart matters - should be before MapControllers)
app.UseCors("AllowAngularApp");

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();

    app.ApplyMigrations();
    app.UseDeveloperExceptionPage();
}


app.UseMiddleware<OpenTelemetryLoggingMiddleware>();
app.UseSerilogRequestLogging();
app.UseRequestContextLogging();
app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// Add our custom middleware AFTER authentication but BEFORE controllers
app.UseTokenValidation();

app.MapControllers();

try
{
    Log.Information("Starting web host");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

// REMARK: Required for functional and integration tests to work.
namespace WebApi
{
    public partial class Program;
}








//static async Task SeedDataAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
//{
//    // Create default roles
//    var roles = new[]
//    {
//        ("Admin", "System Administrator", 1, true),
//        ("User", "Regular User", 2, false),
//        ("Manager", "Manager Role", 3, false)
//    };

//    foreach (var (name, description, priority, isSystem) in roles)
//    {
//        if (!await roleManager.RoleExistsAsync(name))
//        {
//            var role = new ApplicationRole(name)
//            {
//                Description = description,
//                Priority = priority,
//                IsSystemRole = isSystem
//            };
//            await roleManager.CreateAsync(role);
//        }
//    }

//    // Assign all permissions to Admin role
//    var adminRole = await roleManager.FindByNameAsync("Admin");
//    if (adminRole != null)
//    {
//        var allPermissions = await context.Permissions.ToListAsync();
//        foreach (var permission in allPermissions)
//        {
//            var exists = await context.RolePermissions
//                .AnyAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == permission.Id);

//            if (!exists)
//            {
//                context.RolePermissions.Add(new RolePermission
//                {
//                    RoleId = adminRole.Id,
//                    PermissionId = permission.Id
//                });
//            }
//        }
//        await context.SaveChangesAsync();
//    }
//}
