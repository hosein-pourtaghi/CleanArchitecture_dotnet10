// Program.cs - WebApi Project
// ============================================================
// PURPOSE: Application entry point with DI configuration
// This file configures all services, middleware, and pipeline
// ============================================================

using Application;
using Application.Common.Interfaces.Core;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using SharedApi.Extensions;
using SharedKernel.LoggingCore.DependencyInjection;
using WebApi;
using WebApi.Extensions;

// ============================================================
// STEP 1: Create WebApplicationBuilder and configure defaults
// ============================================================
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ============================================================
// STEP 2: Configure Serilog (using library extension)
// ============================================================
Log.Logger = new LoggerConfiguration()
    .InitializeSerilog(builder.Configuration, builder.Environment.ApplicationName)
    .CreateLogger();

// ============================================================
// STEP 3: Configure Host with Serilog
// ============================================================
builder.Host.AddSerilogLogging(builder.Configuration, builder.Environment.ApplicationName);

// ============================================================
// STEP 4: Chain all dependency injection extensions
// ============================================================
builder.Services
    .AddApplication()      // Application layer (MediatR, AutoMapper, Validators)
    .AddPresentation()     // WebApi layer (Controllers, Swagger, HttpClients)
    .AddInfrastructure(builder.Configuration); // Infrastructure layer (DB, Auth, Repos)

builder.AddServiceDefaults();

// ============================================================
// STEP 5: Configure Logging Library (Serilog + OpenTelemetry + Services)
// ============================================================
string? loggingConnectionString = builder.Configuration.GetConnectionString("LoggingConnection");
if (string.IsNullOrEmpty(loggingConnectionString))
{
    throw new InvalidOperationException(
        "LoggingConnection string is not configured. " +
        "Add 'ConnectionStrings:LoggingConnection' to your configuration.");
}

// Single call to add everything: Serilog, OpenTelemetry, Logging Services, DbContext
builder.Services.AddLoggingLibrary(
    builder.Configuration,
    builder.Environment.ApplicationName,
    loggingConnectionString,
    options =>
    {
        options.EnableApiLogging = true;
        options.EnableExceptionLogging = true;
        options.EnablePerformanceLogging = true;
        options.EnableQueryLogging = true;
        options.SlowQueryThresholdMs = 500;
    });

// Register HTTP context accessor for logging
builder.Services.AddHttpContextAccessor();


// ============================================================
// STEP 8: Build Application
// ============================================================
WebApplication app;
try
{
    app = builder.Build();
}
catch (Exception ex)
{
    // Log the exception before re-throwing (for startup failures)
    Log.Fatal(ex, "Application terminated due to an unhandled exception during startup");
    throw;
}

// ============================================================
// STEP 9: Initialize Services on Startup
// ============================================================

//// 9a. Discover and register authorization policies
//using (var scope = app.Services.CreateScope())
//{
//    var policyService = scope.ServiceProvider.GetRequiredService<IPolicyDiscoveryService>();
//    var result = await policyService.DiscoverAndRegisterPoliciesAsync();

//    if (result.IsSuccess && result.Value > 0)
//    {
//        Log.Information("Registered {PolicyCount} authorization policies", result.Value);
//    }
//    else if (result.IsFailure)
//    {
//        Log.Warning("Policy discovery failed: {Error}", result.Error.Description);
//    }
//}

// ============================================================
// STEP 10: Configure Middleware Pipeline
// ============================================================

// 10a. CORS - Must be before routing and endpoints
app.UseCors("AllowAngularApp");

// 10c. Exception Handling - Must be FIRST in the pipeline
// This middleware handles all exceptions globally
app.UseLoggingLibrary();

// 10f. Development-specific configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();
    app.ApplyMigrations();
    app.UseDeveloperExceptionPage();
}
 

// 10g. Exception Handler (fallback for unhandled exceptions)
app.UseExceptionHandler();

// 10h. Routing
app.UseRouting();

// 10i. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 10j. Custom Token Validation Middleware
// NOTE: After authentication but before controllers
//app.UseTokenValidation();

// 10k. Map Controllers
app.MapControllers();

// ============================================================
// STEP 11: Initialize Logging Database
// ============================================================
await app.Services.InitializeLoggingDatabaseAsync();

// ============================================================
// STEP 12: Run Application
// ============================================================
try
{
    Log.Information("Starting {ApplicationName} web host", builder.Environment.ApplicationName);
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    // Ensure all logs are flushed before shutdown
    await Log.CloseAndFlushAsync();
}

// ============================================================
// REMARK: Required for functional and integration tests to work.
// This partial class allows test projects to access the Program class.
// ============================================================
namespace WebApi
{
    public partial class Program { }
}
