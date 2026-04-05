using Domain.Users;

namespace Application.Common.Authentication;

public interface ITokenProvider
{
    string Create(ApplicationUser user);
}
