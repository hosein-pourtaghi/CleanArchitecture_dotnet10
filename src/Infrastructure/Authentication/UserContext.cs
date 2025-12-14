using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

public sealed class UserContext : IUserContext
{
    public sealed class UserContextUnavailableException : Exception
    {
        public UserContextUnavailableException() : base("User context is unavailable")
        {
        }
    }

    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetUserId() ??
        Guid.Empty; //throw new UserContextUnavailableException();
}
