namespace Application.Common.Authentication;


public interface IUserContext
{
    Guid UserId { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
}
