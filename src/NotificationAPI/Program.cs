using System.Threading.Channels;
using Application.Common.DTOs;
using Infrastructure.Messaging;
using Infrastructure.Services;
using NotificationAPI;
using NotificationAPI.Extensions;
using SharedApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// ═══════════════════════════════════════════════════════════════════════════
// Configuration
// ═══════════════════════════════════════════════════════════════════════════

builder.Services.Configure<NotificationApiOptions>(
    builder.Configuration.GetSection(NotificationApiOptions.SectionName));


builder.Services.AddControllers();

builder.Services.AddPresentation();




// ═══════════════════════════════════════════════════════════════════════════
// SignalR
// ═══════════════════════════════════════════════════════════════════════════

builder.Services.AddSignalR(options =>
{
    // Maximum message size for SignalR (default is 32KB)
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB

    // Enable detailed errors (only in development)
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();

    // Keep alive interval
    options.KeepAliveInterval = TimeSpan.FromMinutes(1);

    // Client timeout
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);

    // Handshake timeout
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
})
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.PayloadSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
});

 
// تنظیم Channel (محدود به 100 اعلان)
builder.Services.AddSingleton<Channel<Notification>>(sp => Channel.CreateUnbounded<Notification>());

// ثبت سرویس با نام کامل (Infrastructure.Services.NotificationService)
builder.Services.AddSingleton<NotificationService>();

var app = builder.Build();

app.UseSwaggerWithUi();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();


// ═══════════════════════════════════════════════════════════════════════════
// SignalR Hub Mapping
// ═══════════════════════════════════════════════════════════════════════════

app.MapHub<NotificationHub>("/hubs/notifications")
    .RequireCors("AllowAngularApp");


app.MapControllers();

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
}
