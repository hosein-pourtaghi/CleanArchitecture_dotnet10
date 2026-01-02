using Microsoft.AspNetCore.Identity;

namespace Domain.Users;

public class ApplicationUser : IdentityUser<Guid>
{
    // Add domain-specific properties here (e.g., DisplayName)
    public string DisplayName { get; set; } = string.Empty; 
    public string FirstName { get; set; } 
    public string LastName { get; set; } 
   
  
}
