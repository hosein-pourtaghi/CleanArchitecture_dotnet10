using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using Domain.Entities;

namespace Infrastructure.Services;
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    public AuditMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        // read request info (non-destructive)
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;
        var qs = context.Request.QueryString.Value ?? string.Empty;
        string body = string.Empty;

        if (context.Request.ContentLength > 0 && context.Request.Body.CanSeek == false)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        // exec
        await _next(context);

        // store audit (best effort, don't fail request)
        try
        {
            var log = new AuditLog
            {
                UserId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                Path = path,
                Method = method,
                QueryString = qs,
                Body = body,
                StatusCode = context.Response?.StatusCode ?? 0,
                Timestamp = DateTime.UtcNow
            };
            db.Add(log);
            await db.SaveChangesAsync();
        }
        catch { /* swallow - auditing must not break app */ }
    }
}
