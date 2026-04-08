using System.Security.Claims;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Middleware;

 
public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenValidationMiddleware> _logger;

    public TokenValidationMiddleware(
        RequestDelegate next,
        ILogger<TokenValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        // Only check for authenticated requests
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tokenId = context.User.FindFirst("jti")?.Value;
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenVersionClaim = context.User.FindFirst("token_version")?.Value;

            _logger.LogDebug("Validating token. TokenId: {TokenId}, UserId: {UserId}",
                tokenId, userIdClaim);

            // 1. Check if token is blacklisted
            if (!string.IsNullOrEmpty(tokenId))
            {
                var isBlacklisted = await dbContext.TokenBlacklist
                    .AnyAsync(t => t.TokenId == tokenId && t.ExpiresAt > DateTime.UtcNow);

                if (isBlacklisted)
                {
                    _logger.LogWarning("Token {TokenId} is blacklisted. User: {UserId}",
                        tokenId, userIdClaim);

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Token has been revoked",
                        code = "TOKEN_REVOKED"
                    });
                    return;
                }
            }

            // 2. Check token version
            if (!string.IsNullOrEmpty(userIdClaim) && !string.IsNullOrEmpty(tokenVersionClaim))
            {
                var user = await dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userIdClaim));

                if (user != null && user.TokenVersion != int.Parse(tokenVersionClaim))
                {
                    _logger.LogWarning("Token version mismatch for user {UserId}. " +
                        "Expected: {Expected}, Got: {Got}",
                        userIdClaim, user.TokenVersion, tokenVersionClaim);

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Token has been revoked. Please login again.",
                        code = "TOKEN_VERSION_MISMATCH"
                    });
                    return;
                }
            }
        }

        await _next(context);
    }
}
