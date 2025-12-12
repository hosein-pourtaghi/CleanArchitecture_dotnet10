
namespace Domain.Users;

public class ApplicationUser //: IdentityUser
{
    // Add domain-specific properties here (e.g., DisplayName)
    public long Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string UserName { get; set; }  
    public string Email { get; set; }  
}
