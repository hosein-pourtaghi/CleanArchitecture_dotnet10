using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;
 

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // 🔥 CACHE: Store claims to avoid repeated lookups
    private readonly Lazy<string?> _userId;
    private readonly Lazy<string?> _email;
    private readonly Lazy<IReadOnlyList<string>> _roles;
    private readonly Lazy<bool> _isAuthenticated;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        // 🔥 LAZY LOAD: Only access HttpContext when needed
        _userId = new Lazy<string?>(GetUserId);
        _email = new Lazy<string?>(GetEmail);
        _roles = new Lazy<IReadOnlyList<string>>(GetRoles);
        _isAuthenticated = new Lazy<bool>(() => !string.IsNullOrEmpty(UserId));
    }

    public string? UserId => _userId.Value;
    public string? Email => _email.Value;
    public IReadOnlyList<string> Roles => _roles.Value;
    public bool IsAuthenticated => _isAuthenticated.Value;

    public bool HasRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasAnyRole(params string[] roles)
    {
        return roles.Any(r => HasRole(r));
    }

    #region Private Methods

    private string? GetUserId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User == null)
            return null;

        // Try to get user ID from different claim types (flexible for different auth schemes)
        return context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue("uid")
            ?? context.User.FindFirstValue(ClaimTypes.Name);
    }

    private string? GetEmail()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User == null)
            return null;

        return context.User.FindFirstValue(ClaimTypes.Email)
            ?? context.User.FindFirstValue("email")
            ?? context.User.FindFirstValue(ClaimTypes.Name);
    }

    private IReadOnlyList<string> GetRoles()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User == null)
            return Array.Empty<string>();

        // Get all role claims
        var roleClaims = context.User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Also check for "roles" claim (some JWT providers use this)
        var rolesClaim = context.User.FindFirstValue("roles");
        if (!string.IsNullOrEmpty(rolesClaim))
        {
            var additionalRoles = rolesClaim.Split(',', StringSplitOptions.RemoveEmptyEntries);
            roleClaims.AddRange(additionalRoles);
        }

        return roleClaims.Distinct().ToList();
    }

    #endregion
}
