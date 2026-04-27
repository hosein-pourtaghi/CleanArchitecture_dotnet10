using IdentityApi.Application.Interfaces;
using IdentityApi.Infrastructure.Authorization;
using IdentityApi.Infrastructure.Persistence;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using SharedKernel.LoggingCore.DependencyInjection;
 

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


// ============================================
// 2. Add Identity Infrastructure (all in one call)
// ============================================
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// ============================================
// 3. Add Application Services (if any)
// ============================================
// builder.Services.AddApplicationServices();

// ============================================
// 4. Add Controllers & API Features
// ============================================
builder.Services.AddControllers();

// ============================================
// 5. Add Swagger/OpenAPI
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity API",
        Version = "v1",
        Description = "Authentication & Authorization API"
    });

    // JWT Bearer authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================
// 6. Add Health Checks (optional)
// ============================================
// builder.Services.AddHealthChecks();

// ============================================
// 7. Add CORS (if needed)
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        // For development - allows any origin
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();

        // For production with specific origins:
        // policy.WithOrigins("https://yourdomain.com")
        //       .AllowAnyHeader()
        //       .AllowAnyMethod();

        // For credentials-based auth (uncomment if needed):
        // policy.AllowCredentials(); // Cannot be used with AllowAnyOrigin()
    });
});

var app = builder.Build();

// ============================================
// Configure HTTP Pipeline
// ============================================
 

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
    });
}

// HTTPS redirection
app.UseHttpsRedirection();





// ============================================================
// STEP 9: Initialize Services on Startup
// ============================================================

//// 9a. Discover and register authorization policies
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





// CORS (if enabled)
app.UseCors("AllowAngularApp");

// Authentication & Authorization (order matters!)
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check endpoint (if enabled)
// app.MapHealthChecks("/health");

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

Log.Information("Identity API started");

try
{
    app.Run();
}
catch (Exception e)
{

    throw;
}

