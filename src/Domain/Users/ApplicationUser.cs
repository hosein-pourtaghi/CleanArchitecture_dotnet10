using Microsoft.AspNetCore.Identity;

namespace Domain.Users;

public class ApplicationUser : IdentityUser<Guid>
{
    // Add domain-specific properties here (e.g., DisplayName)
    public string DisplayName 
    { 
        get => $"{FirstName} {LastName}".Trim(); 
    }
    public string? FirstName { get; set; } 
    public string? LastName { get; set; } 
   
  
}
