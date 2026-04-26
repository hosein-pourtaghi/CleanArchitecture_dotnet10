// Infrastructure/Authorization/IdentitySeeder.cs
using IdentityApi.Application.Interfaces;
using IdentityApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace IdentityApi.Infrastructure.Authorization;

public interface IIdentitySeeder
{
    Task SeedAdminUserAsync(string email, string password, string roleName = "Admin");
}

public class IdentitySeeder : IIdentitySeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IMyIdentityDbContext _context;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IMyIdentityDbContext context,
        ILogger<IdentitySeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    public async Task SeedAdminUserAsync(string email, string password, string roleName = "Admin")
    {
        // 1. Create admin role if it doesn't exist
        var adminRole = await _roleManager.FindByNameAsync(roleName);
        if (adminRole == null)
        {
            adminRole = new ApplicationRole(roleName)
            {
                Description = "System administrator with full access",
                IsSystemRole = true,
                Priority = 0 // Highest priority
            };

            var roleResult = await _roleManager.CreateAsync(adminRole);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, errors);
                return;
            }
            _logger.LogInformation("Created role: {RoleName}", roleName);
        }
        else
        {
            _logger.LogInformation("Role {RoleName} already exists", roleName);
        }

        // 2. Create admin user if it doesn't exist
        var adminUser = await _userManager.FindByEmailAsync(email);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                AllowMultipleSessions = true,
                MaxConcurrentSessions = 10
            };

            var userResult = await _userManager.CreateAsync(adminUser, password);
            if (!userResult.Succeeded)
            {
                var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user {Email}: {Errors}", email, errors);
                return;
            }
            _logger.LogInformation("Created admin user: {Email}", email);
        }
        else
        {
            _logger.LogInformation("User {Email} already exists", email);
        }

        // 3. Assign role to user if not already assigned
        var isInRole = await _userManager.IsInRoleAsync(adminUser, roleName);
        if (!isInRole)
        {
            var addRoleResult = await _userManager.AddToRoleAsync(adminUser, roleName);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to add role {RoleName} to user {Email}: {Errors}", roleName, email, errors);
                return;
            }
            _logger.LogInformation("Assigned role {RoleName} to user {Email}", roleName, email);
        }

        // 4. Assign ALL permissions to admin role
        await AssignAllPermissionsToRoleAsync(adminRole);
    }

    private async Task AssignAllPermissionsToRoleAsync(ApplicationRole role)
    {
        // Get all permissions from database
        var allPermissions = await _context.Permissions.ToListAsync();

        if (!allPermissions.Any())
        {
            _logger.LogWarning("No permissions found in database. Run policy discovery first.");
            return;
        }

        // Get existing role permissions to avoid duplicates
        var existingRolePermissionIds = await _context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var newPermissions = allPermissions
            .Where(p => !existingRolePermissionIds.Contains(p.Id))
            .ToList();

        if (!newPermissions.Any())
        {
            _logger.LogInformation("Role {RoleName} already has all {Count} permissions", role.Name, allPermissions.Count);
            return;
        }

        foreach (var permission in newPermissions)
        {
            var rolePermission = new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            };
            _context.RolePermissions.Add(rolePermission);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation(
            "Assigned {NewCount} new permissions to role {RoleName} (total: {TotalCount})",
            newPermissions.Count, role.Name, allPermissions.Count);
    }
}
