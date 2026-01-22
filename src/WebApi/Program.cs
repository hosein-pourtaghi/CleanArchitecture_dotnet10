using System.Reflection;
using Application;
using Application.Common.Mappings;
using FluentValidation;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Serilog;
using WebApi;
using WebApi.Extensions;
using WebApi.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
string? connectionString = builder.Configuration["ConnectionString"];
// 1. Configure Serilog to write to SQL Server 
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MyAspireApp")
    //.WriteTo.MSSqlServer(
    //    connectionString: connectionString,
    //    tableName: "AppLogs",
    //    autoCreateSqlTable: true, // Automatically creates the table on startup
    //    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
    //)
    .CreateLogger(); 

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration)); 

// Keep OpenTelemetry for Traces (Aspire Dashboard)
builder.Services.AddOpenTelemetry()
    .UseOtlpExporter()  
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("WebApi.*") // Your app's source
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); 


builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

// automapper & validators
builder.Services.AddAutoMapper(typeof(CustomerProfile));
builder.Services.AddValidatorsFromAssembly(typeof(Application.Common.Mappings.CustomerProfile).Assembly);


WebApplication app = builder.Build();

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

// Add OpenTelemetry logging middleware before other logging middleware
app.UseMiddleware<OpenTelemetryLoggingMiddleware>();


// Standard Request Logging (Headers, Path, Status) ***Add this BEFORE other middlewares (like MapControllers)***
app.UseSerilogRequestLogging();

app.UseRequestContextLogging();

app.UseExceptionHandler();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();




//app.MapHub<Infrastructure.Services.NotificationHub>("/hubs/notifications");


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
