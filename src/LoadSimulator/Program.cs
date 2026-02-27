using LoadSimulator;
using LoadSimulator.Infrastructure;
using LoadSimulator.Services;
using LoadSimulator.Configuration;
using LoadSimulator.BackgroundServices;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;
using Polly.Extensions.Http;
using Polly.CircuitBreaker;
using Swashbuckle.AspNetCore.SwaggerGen;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "LoadSimulator")
    .CreateLogger();

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

// Configuration
builder.Services.Configure<SimulatorSettings>(builder.Configuration.GetSection("Simulator"));
builder.Services.Configure<PrometheusSettings>(builder.Configuration.GetSection("Prometheus"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Register Redis (optional)
if (builder.Configuration.GetValue<bool>("Redis:Enabled"))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var options = ConfigurationOptions.Parse(builder.Configuration["Redis:ConnectionString"]!);
        return ConnectionMultiplexer.Connect(options);
    });
}

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("LoadSimulator")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    })
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation()
            .AddPrometheusExporter();
    });

// Health Checks
var healthCheckTags = new[] { "live" };
var healthChecksBuilder = builder.Services.AddHealthChecks()
    .AddCheck("self", () => new HealthCheckResult(HealthStatus.Healthy), 
        tags: healthCheckTags);

if (builder.Configuration.GetValue<bool>("Redis:Enabled"))
{
    healthChecksBuilder.AddRedis(builder.Configuration["Redis:ConnectionString"]!);
}

if (builder.Configuration.GetValue<bool>("Logging:Database:Enabled"))
{
    var dbType = builder.Configuration["Logging:Database:Type"];
    if (dbType == "PostgreSQL")
    {
        healthChecksBuilder.AddNpgSql(builder.Configuration["Logging:Database:ConnectionString"]!);
    }
    else if (dbType == "SqlServer")
    {
        healthChecksBuilder.AddSqlServer(builder.Configuration["Logging:Database:ConnectionString"]!);
    }
}

// HTTP Clients
builder.Services.AddHttpClient("MainApi")
    .ConfigureHttpClient(client =>
    {
        var settings = builder.Configuration.GetSection("Simulator").Get<SimulatorSettings>();
        client.BaseAddress = new Uri(settings!.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy());

builder.Services.AddHttpClient<IAuthClient, AuthClient>()
    .ConfigureHttpClient(client =>
    {
        var settings = builder.Configuration.GetSection("Simulator").Get<SimulatorSettings>();
        client.BaseAddress = new Uri(settings!.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy());

builder.Services.AddHttpClient<IProductClient, ProductClient>()
    .ConfigureHttpClient(client =>
    {
        var settings = builder.Configuration.GetSection("Simulator").Get<SimulatorSettings>();
        client.BaseAddress = new Uri(settings!.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy());

builder.Services.AddHttpClient<IOrderClient, OrderClient>()
    .ConfigureHttpClient(client =>
    {
        var settings = builder.Configuration.GetSection("Simulator").Get<SimulatorSettings>();
        client.BaseAddress = new Uri(settings!.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy());

// Services
builder.Services.AddSingleton<IProductCacheService, ProductCacheService>();
builder.Services.AddScoped<IUserSimulationService, UserSimulationService>();
builder.Services.AddSingleton<SimulationMetricsService>();
builder.Services.AddSingleton(MockDataGenerator.Instance);

// Background Service
builder.Services.AddHostedService<SimulationBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();
app.MapHealthChecks("health");
app.MapPrometheusScrapingEndpoint();
app.MapControllers();

try
{
    Log.Information("Starting Load Simulator");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Load Simulator terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
