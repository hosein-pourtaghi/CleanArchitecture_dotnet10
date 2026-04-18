using Application;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Authorization;
using LoggingCore.DependencyInjection;
using LoggingCore.Middleware;
using MediatRCore.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using WebApi;
using WebApi.Extensions;
using WebApi.Middleware;
using WebApi.Telemetry;


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

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MyAspireApp")
    .CreateLogger();

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("WebApi")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    })
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder
            .AddRuntimeInstrumentation()
            .AddOtlpExporter();
    });

// from appsettings
//builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Host.UseSerilog();


// add this to infra layer to get all webAPI projects
builder.Services.AddScoped<IPolicyDiscoveryService, PolicyDiscoveryService>();


builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

#region add Exception handling LIB

// ==================== Database ====================

builder.Services.AddLoggingDbContext(builder.Configuration.GetConnectionString("LoggingConnection")!);
  
// ==================== Logging Services ====================

builder.Services.AddLoggingServices(options =>
{
    options.EnableApiLogging = true;
    options.EnableExceptionLogging = true;
    options.EnablePerformanceLogging = true;
    options.EnableQueryLogging = builder.Environment.IsDevelopment();  // Only in dev
    options.SlowQueryThresholdMs = 500;  // Log queries > 500ms
    options.ShowDetailsInProduction = builder.Environment.IsDevelopment();
    options.BatchSize = 100;
    options.BatchIntervalMs = 1000;
    options.MaxQueueSize = 10000; 
});

// ==================== MediatR ====================

builder.Services.AddMediatRWithBehaviors(
    typeof(Program).Assembly,
    typeof(Application.DependencyInjection).Assembly);

builder.Services.AddFluentValidationWithMediatR(
    typeof(Program).Assembly,
    typeof(Application.DependencyInjection).Assembly);

// ==================== Other Services ====================

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TelemetryActivitySource.Instance);
#endregion



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

// Important: Order matters!

// 1. Exception handling (must be first)
app.UseMiddleware<ExceptionHandlingMiddleware>();


// 2. OpenTelemetry
app.UseMiddleware<OpenTelemetryLoggingMiddleware>();

// 3. Serilog request logging
app.UseSerilogRequestLogging();

//app.UseRequestContextLogging();
app.UseExceptionHandler();
// 4. Routing and authentication
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// Add our custom middleware AFTER authentication but BEFORE controllers
app.UseTokenValidation();

app.MapControllers();


// ==================== Initialize Logging Database ====================
app.Services.InitializeLoggingDatabaseAsync();



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
