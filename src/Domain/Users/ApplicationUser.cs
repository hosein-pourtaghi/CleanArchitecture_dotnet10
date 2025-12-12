
namespace Domain.Users;

public class ApplicationUser //: IdentityUser
{
    // Add domain-specific properties here (e.g., DisplayName)
    public string DisplayName { get; set; } = string.Empty;
}
