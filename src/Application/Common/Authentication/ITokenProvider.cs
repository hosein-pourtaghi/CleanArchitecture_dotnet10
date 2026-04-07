using Domain.Entities.Identities;

namespace Application.Common.Authentication;

public interface ITokenProvider
{
    string Create(ApplicationUser user);
}
