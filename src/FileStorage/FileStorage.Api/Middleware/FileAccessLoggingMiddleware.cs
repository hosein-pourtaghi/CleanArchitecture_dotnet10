// FileStorage.Api/Middleware/FileAccessLoggingMiddleware.cs
using System.Diagnostics;
using FileStorage.Application.Options;
using FileStorage.Domain.Enums;
using FileStorage.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Options;

namespace FileStorage.Api.Middleware;

public class FileAccessLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly FileAccessLoggingOptions _options;
    private readonly ILogger<FileAccessLoggingMiddleware> _logger;

    private static readonly HashSet<string> FilePaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/files",
        "/files"
    };

    public FileAccessLoggingMiddleware(
        RequestDelegate next,
        IOptions<FileAccessLoggingOptions> options,
        ILogger<FileAccessLoggingMiddleware> logger)
    {
        _next = next;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IFileAccessLogRepository accessLogRepository,
        IFileAttachmentRepository fileRepository)
    {
        // Only log file-related requests
        var path = context.Request.Path.Value ?? string.Empty;
        if (!FilePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var fileId = ExtractFileId(path);
        var action = DetermineAction(context, path);
        var success = context.Response.StatusCode < 400;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Determine if we should log this request
            if (ShouldLog(context, success))
            {
                await LogAccessAsync(
                    context,
                    fileRepository,
                    accessLogRepository,
                    fileId,
                    action,
                    success,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }

    private static Guid? ExtractFileId(string path)
    {
        // Extract GUID from path like /api/files/{guid}
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i].Equals("files", StringComparison.OrdinalIgnoreCase) &&
                i + 1 < segments.Length)
            {
                if (Guid.TryParse(segments[i + 1], out var guid))
                    return guid;
            }
        }

        return null;
    }

    private static FileAccessAction DetermineAction(HttpContext context, string path)
    {
        var method = context.Request.Method;
        var lowerPath = path.ToLowerInvariant();

        if (method == "GET")
        {
            if (lowerPath.Contains("/thumbnail"))
                return FileAccessAction.Viewed;
            if (lowerPath.Contains("/download") || lowerPath.Contains("/stream"))
                return FileAccessAction.Downloaded;
            return FileAccessAction.Viewed;
        }

        if (method == "POST")
        {
            if (lowerPath.Contains("/upload"))
                return FileAccessAction.Uploaded;
            return FileAccessAction.Viewed;
        }

        if (method == "DELETE")
        {
            return FileAccessAction.Deleted;
        }

        return FileAccessAction.Viewed;
    }

    private bool ShouldLog(HttpContext context, bool success)
    {
        if (!_options.Enabled)
            return false;

        // Check if file is public
        var isPublicFile = context.Request.Headers.TryGetValue("X-File-Access-Level", out var accessLevel) &&
                           accessLevel == "Public";

        // Log all access if configured
        if (_options.LogAllAccess)
            return true;

        // Log failed access if configured
        if (!success && _options.LogFailedAccess)
            return true;

        // Log public file access if configured
        if (isPublicFile && _options.LogPublicFileAccess)
            return true;

        return false;
    }

    private async Task LogAccessAsync(
        HttpContext context,
        IFileAttachmentRepository fileRepository,
        IFileAccessLogRepository accessLogRepository,
        Guid? fileId,
        FileAccessAction action,
        bool success,
        long responseTimeMs)
    {
        try
        {
            Guid? ownerId = null;
            string? ownerType = null;

            if (fileId.HasValue)
            {
                var file = await fileRepository.GetByIdAsync(fileId.Value);
                if (file != null)
                {
                    ownerId = file.OwnerId;
                    ownerType = file.OwnerType;
                }
            }

            var userId = GetUserId(context);
            var ipAddress = GetClientIp(context);
            var userAgent = context.Request.Headers.UserAgent.ToString();

            var log = success
                ? FileStorage.Domain.Entities.FileAccessLog.Create(
                    fileId ?? Guid.Empty,
                    action,
                    true,
                    userId,
                    ipAddress,
                    userAgent,
                    null,
                    ownerId,
                    ownerType,
                    context.Request.Path,
                    context.Request.Method,
                    context.Response.StatusCode,
                    responseTimeMs)
                : FileStorage.Domain.Entities.FileAccessLog.CreateFailed(
                    fileId ?? Guid.Empty,
                    action,
                    $"HTTP {context.Response.StatusCode}",
                    userId,
                    ipAddress,
                    userAgent,
                    ownerId,
                    ownerType);

            await accessLogRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log file access");
        }
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? context.User.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }

    private static string? GetClientIp(HttpContext context)
    {
        // Check for forwarded headers (reverse proxy)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(ip))
                return ip;
        }

        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            return realIp.FirstOrDefault();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}

public class FileAccessLoggingOptions
{
    public bool Enabled { get; set; } = true;
    public bool LogAllAccess { get; set; } = true;
    public bool LogPublicFileAccess { get; set; } = true;
    public bool LogFailedAccess { get; set; } = true;
}

public static class FileAccessLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseFileAccessLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FileAccessLoggingMiddleware>();
    }
}
