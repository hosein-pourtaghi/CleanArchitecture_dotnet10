using System.Threading.Channels;
using Application.Common.DTOs;
using Infrastructure.Services;
using NotificationAPI;
using NotificationAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddPresentation();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

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

app.MapControllers();

app.Run();
