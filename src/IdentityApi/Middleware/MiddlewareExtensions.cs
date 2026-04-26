// src/WebApi/Middleware/MiddlewareExtensions.cs
// ============================================================
// PURPOSE: Extension methods for middleware registration
// ============================================================

namespace IdentityApi.Middleware;

/// <summary>
/// Extension methods for configuring middleware.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds the token validation middleware to the pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    /// <remarks>
    /// This middleware should be placed AFTER authentication but BEFORE controllers.
    /// It performs additional token validation such as checking token blacklist.
    /// </remarks>
    public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenValidationMiddleware>();
    }
}
