// FileStorage.Api/Extensions/ServiceCollectionExtensions.cs
using System.Collections.Generic;
using System.Security.Claims;
using FileStorage.Application.Options;
using FileStorage.Application.Services;
using FileStorage.Domain.Interfaces;
using FileStorage.Domain.Interfaces.Repositories;
using FileStorage.Infrastructure.Persistence;
using FileStorage.Infrastructure.Persistence.Repositories;
using FileStorage.Infrastructure.Storage.Providers;
using FileStorage.Infrastructure.Thumbnails;
using FileStorage.Infrastructure.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FileStorage.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileStorageServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration
        var options = new FileStorageOptions();
        configuration.GetSection("FileStorage").Bind(options);
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));

        // Register DbContext
        services.AddDbContext<FileStorageDbContext>(dbOptions =>
        {
            dbOptions.UseSqlServer(
                configuration.GetConnectionString("FileStorage"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(30);
                });
        });

        // Register repositories
        services.AddScoped<IFileAttachmentRepository, FileAttachmentRepository>();
        services.AddScoped<IFileHistoryRepository, FileHistoryRepository>();
        services.AddScoped<IFileAccessLogRepository, FileAccessLogRepository>();
        services.AddScoped<IFileAccessPermissionRepository, FileAccessPermissionRepository>();

        // Register storage provider
        services.AddSingleton<IFileStorageProvider>(sp =>
        {
            var fileOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FileStorageOptions>>();
            var logger = sp.GetService<ILogger<LocalStorageProvider>>();

            return new LocalStorageProvider(
                //rootPath: Path.Combine(Directory.GetCurrentDirectory(), fileOptions.Value.Storage.Local.RootPath),
                rootPath: Path.Combine(  fileOptions.Value.Storage.Local.RootPath),
                baseUrl: fileOptions.Value.Storage.BaseUrl,
                bufferSize: fileOptions.Value.Performance.BufferSize,
                logger: logger);
        });

        // Register validators
        services.AddSingleton<MagicByteValidator>();
        services.AddSingleton<ExecutableSignatureDetector>();
        services.AddSingleton<IFileValidator, FileValidator>();

        // Register thumbnail generator
        services.AddSingleton<IThumbnailGenerator, ImageThumbnailGenerator>();

        // Register icon provider
        services.AddSingleton<IFileIconProvider>(sp =>
        {
            var logger = sp.GetService<ILogger<FileIconProvider>>();
            var iconsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "icons");
            return new FileIconProvider(iconsPath, logger);
        });

        // Register current user service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register main service
        services.AddScoped<IFileManagerService, FileManagerService>();

        return services;
    }

    public static async Task InitializeFileStorageDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileStorageDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FileStorageDbContext>>();

        try
        {
            logger.LogInformation("Ensuring FileStorage database is created...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("FileStorage database ready.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing FileStorage database");
            throw;
        }
    }
}
