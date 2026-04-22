using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FileStorage.Application.Services;


public interface ICurrentUserService
{
    Guid GetUserId();
    bool IsAdmin();
    bool IsAuthenticated();
}
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // 🔥 CACHE: Store claims to avoid repeated lookups
    private readonly Lazy<Guid> _userId;
    private readonly Lazy<string?> _email;
    private readonly Lazy<IReadOnlyList<string>> _roles;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        // 🔥 LAZY LOAD: Only access HttpContext when needed
        _userId = new Lazy<Guid>(GetUserId);
        _email = new Lazy<string?>(GetEmail);
        _roles = new Lazy<IReadOnlyList<string>>(GetRoles);
    }

    public Guid UserId => _userId.Value;
    public string? Email => _email.Value;
    public IReadOnlyList<string> Roles => _roles.Value;

    public bool HasRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasAnyRole(params string[] roles)
    {
        return roles.Any(r => HasRole(r));
    }

    #region Private Methods


    public Guid GetUserId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User == null)
            return Guid.Empty;

        // Try to get user ID from different claim types (flexible for different auth schemes)
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue("uid")
            ?? context.User.FindFirstValue(ClaimTypes.Name);
        var isId = Guid.TryParse(userId, out Guid uId);
        return isId ? uId : Guid.Empty;
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

    public bool IsAdmin()
    {
        return HasRole("Admin");
    }

    public bool IsAuthenticated()
    {
        return UserId != Guid.Empty;
    }

}




//public class HttpContextCurrentUserService : ICurrentUserService
//{
//    private readonly IHttpContextAccessor _httpContextAccessor;

//    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
//    {
//        _httpContextAccessor = httpContextAccessor;
//    }

//    public Guid? GetUserId()
//    {
//        var user = _httpContextAccessor.HttpContext?.User;
//        if (user == null || !user.Identity?.IsAuthenticated ?? true)
//            return null;

//        var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
//                          ?? user.FindFirst("sub")?.Value;

//        if (Guid.TryParse(userIdClaim, out var userId))
//            return userId;

//        return null;
//    }

//    public bool IsAdmin()
//    {
//        var user = _httpContextAccessor.HttpContext?.User;
//        return user?.IsInRole("Admin") ?? false;
//    }

//    public bool IsAuthenticated()
//    {
//        return _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
//    }
//}
