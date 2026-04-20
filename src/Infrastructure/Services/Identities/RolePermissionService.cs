using Application.Common.DTOs.Identities;
using Application.Common.Interfaces.Core;
using Domain.Aggregates.Identities;
using Infrastructure.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;


namespace Infrastructure.Services.Identities;



public class RolePermissionService : IRolePermissionService
{
    private readonly IApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly PermissionProvider _permissionProvider;
    public RolePermissionService(
        IApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        PermissionProvider permissionProvider)
    {
        _context = context;
        _roleManager = roleManager;
        _permissionProvider = permissionProvider;
    }

    #region Permissions

    public async Task<Result<List<PermissionDto>>> GetAllPermissionsAsync()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        var result = permissions.Select(p => new PermissionDto(
            p.Id,
            p.Name,
            p.Description,
            p.Category,
            false
        )).ToList();

        return Result.Success(result);
    }

    public async Task<Result<PermissionDto>> GetPermissionByIdAsync(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);

        if (permission == null)
            return Result.Failure<PermissionDto>("Permission not found");

        return Result.Success(new PermissionDto(
            permission.Id,
            permission.Name,
            permission.Description,
            permission.Category,
            false
        ));
    }

    public async Task<Result<PermissionDto>> CreatePermissionAsync(string name, string description, string category)
    {
        // Check for duplicate
        var existing = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());

        if (existing != null)
            return Result.Failure<PermissionDto>("Permission with this name already exists");

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = name.ToLower().Trim(),
            Description = description,
            Category = category,
            CreatedAt = DateTime.UtcNow
        };

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        return Result.Success(new PermissionDto(
            permission.Id,
            permission.Name,
            permission.Description,
            permission.Category,
            false
        ));
    }

    public async Task<Result<PermissionDto>> UpdatePermissionAsync(Guid id, string name, string description, string category)
    {
        var permission = await _context.Permissions.FindAsync(id);

        if (permission == null)
            return Result.Failure<PermissionDto>("Permission not found");

        // Check for duplicate name (excluding current)
        var duplicate = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id != id && p.Name.ToLower() == name.ToLower());

        if (duplicate != null)
            return Result.Failure<PermissionDto>("Permission with this name already exists");

        permission.Name = name.ToLower().Trim();
        permission.Description = description;
        permission.Category = category;
        permission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result.Success(new PermissionDto(
            permission.Id,
            permission.Name,
            permission.Description,
            permission.Category,
            false
        ));
    }

    public async Task<Result> DeletePermissionAsync(Guid id)
    {
        var permission = await _context.Permissions
            .Include(p => p.RolePermissions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (permission == null)
            return Result.Failure("Permission not found");

        // Check if permission is assigned to any role
        if (permission.RolePermissions.Any())
            return Result.Failure("Cannot delete permission that is assigned to roles. Remove role assignments first.");

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    #endregion

    #region Roles

    public async Task<Result<List<RoleDto>>> GetAllRolesAsync()
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync();

        var result = roles.Select(r => new RoleDto(
            r.Id,
            r.Name!,
            r.Description,
            r.IsSystemRole,
            r.Priority,
            r.RolePermissions.Select(rp => new PermissionDto(
                rp.Permission.Id,
                rp.Permission.Name,
                rp.Permission.Description,
                rp.Permission.Category,
                true
            )).ToList(),
            r.UserRoles.Count
        )).ToList();

        return Result.Success(result);
    }

    public async Task<Result<RoleDto>> GetRoleByIdAsync(Guid id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return Result.Failure<RoleDto>("Role not found");

        return Result.Success(MapToRoleDto(role));
    }

    public async Task<Result<RoleDto>> CreateRoleAsync(CreateRoleRequest request)
    {
        // Check for duplicate role name
        var existingRole = await _roleManager.FindByNameAsync(request.Name);
        if (existingRole != null)
            return Result.Failure<RoleDto>("Role with this name already exists");

        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsSystemRole = false,
            Priority = 0,
            NormalizedName = request.Name.ToUpperInvariant()
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<RoleDto>($"Failed to create role: {errors}");
        }

        // Assign permissions if provided
        if (request.PermissionIds?.Any() == true)
        {
            await SetRolePermissionsAsync(role.Id, request.PermissionIds);
        }

        // Reload role with permissions
        var createdRole = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .FirstAsync(r => r.Id == role.Id);

        return Result.Success(MapToRoleDto(createdRole));
    }

    public async Task<Result<RoleDto>> UpdateRoleAsync(Guid id, UpdateRoleRequest request)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return Result.Failure<RoleDto>("Role not found");

        if (role.IsSystemRole)
            return Result.Failure<RoleDto>("Cannot modify system role");

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var existingRole = await _roleManager.FindByNameAsync(request.Name);
            if (existingRole != null && existingRole.Id != id)
                return Result.Failure<RoleDto>("Role with this name already exists");

            role.Name = request.Name;
            role.NormalizedName = request.Name.ToUpperInvariant();
        }

        // Update description if provided
        if (request.Description != null)
        {
            role.Description = request.Description;
        }

        // Update permissions if provided
        if (request.PermissionIds != null)
        {
            await SetRolePermissionsAsync(id, request.PermissionIds);
        }

        await _context.SaveChangesAsync();

        // Reload role with permissions
        var updatedRole = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .FirstAsync(r => r.Id == id);

        return Result.Success(MapToRoleDto(updatedRole));
    }

    public async Task<Result> DeleteRoleAsync(Guid id)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return Result.Failure("Role not found");

        if (role.IsSystemRole)
            return Result.Failure("Cannot delete system role");

        // Check if role has users
        if (role.UserRoles.Any())
            return Result.Failure("Cannot delete role that has users assigned. Remove user assignments first.");

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to delete role: {errors}");
        }

        return Result.Success();
    }

    #endregion

    #region Role-Permission Management

    public async Task<Result> AssignPermissionsToRoleAsync(Guid roleId, List<Guid> permissionIds)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            return Result.Failure("Role not found");

        var permissions = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .ToListAsync();

        if (permissions.Count != permissionIds.Count)
            return Result.Failure("One or more permissions not found");

        foreach (var permission in permissions)
        {
            var existing = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id);

            if (existing == null)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permission.Id
                };
                _context.RolePermissions.Add(rolePermission);
            }
        }

        // Clear cache for all users with this role
        var userIds = await _context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        foreach (var uid in userIds)
        {
            _permissionProvider.ClearCache(uid);
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemovePermissionsFromRoleAsync(Guid roleId, List<Guid> permissionIds)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            return Result.Failure("Role not found");

        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
            .ToListAsync();

        _context.RolePermissions.RemoveRange(rolePermissions);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> SetRolePermissionsAsync(Guid roleId, List<Guid> permissionIds)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            return Result.Failure("Role not found");

        // Get all permissions
        var allPermissions = await _context.Permissions.ToListAsync();

        // Validate permission IDs
        var validPermissionIds = permissionIds.Where(id => allPermissions.Any(p => p.Id == id)).ToList();

        if (validPermissionIds.Count != permissionIds.Count)
            return Result.Failure("One or more permissions not found");

        // Remove existing permissions
        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existingPermissions);

        // Add new permissions
        foreach (var permissionId in validPermissionIds)
        {
            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };
            _context.RolePermissions.Add(rolePermission);
        }

        // Clear cache for all users with this role
        var userIds = await _context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        foreach (var uid in userIds)
        {
            _permissionProvider.ClearCache(uid);
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    #endregion

    #region User-Role Management

    public async Task<Result> AssignRoleToUserAsync(Guid userId, Guid roleId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return Result.Failure("User not found");

        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            return Result.Failure("Role not found");

        // Check if user already has this role
        var existingUserRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (existingUserRole != null)
            return Result.Failure("User already has this role");

        var userRole = new ApplicationUserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        _context.UserRoles.Add(userRole);

        _permissionProvider.ClearCache(userId);

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveRoleFromUserAsync(Guid userId, Guid roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole == null)
            return Result.Failure("User does not have this role");

        _context.UserRoles.Remove(userRole);

        _permissionProvider.ClearCache(userId);

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<List<RoleDto>>> GetUserRolesAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return Result.Failure<List<RoleDto>>("User not found");

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(r => r.Role.RolePermissions).ThenInclude(rp => rp.Permission)
            .Select(ur => ur.Role)
            .ToListAsync();

        var result = roles.Select(r => new RoleDto(
            r.Id,
            r.Name!,
            r.Description,
            r.IsSystemRole,
            r.Priority,
            r.RolePermissions.Select(rp => new PermissionDto(
                rp.Permission.Id,
                rp.Permission.Name,
                rp.Permission.Description,
                rp.Permission.Category,
                true
            )).ToList(),
            0 // User count not needed here
        )).ToList();

        return Result.Success(result);
    }

    public async Task<Result<List<PermissionDto>>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return Result.Failure<List<PermissionDto>>("User not found");

        // Get all unique permissions from user's roles
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        var result = permissions.Select(p => new PermissionDto(
            p.Id,
            p.Name,
            p.Description,
            p.Category,
            true
        )).ToList();

        return Result.Success(result);
    }

    #endregion

    #region Helper Methods

    private static RoleDto MapToRoleDto(ApplicationRole role)
    {
        return new RoleDto(
            role.Id,
            role.Name!,
            role.Description,
            role.IsSystemRole,
            role.Priority,
            role.RolePermissions.Select(rp => new PermissionDto(
                rp.Permission.Id,
                rp.Permission.Name,
                rp.Permission.Description,
                rp.Permission.Category,
                true
            )).ToList(),
            role.UserRoles.Count
        );
    }

    #endregion
}
