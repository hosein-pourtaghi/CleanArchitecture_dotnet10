
using Application.Common.DTOs.Identities;

using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRolePermissionService _rolePermissionService;

    public RolesController(IRolePermissionService rolePermissionService)
    {
        _rolePermissionService = rolePermissionService;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:roles.read")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _rolePermissionService.GetAllRolesAsync();

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:roles.read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _rolePermissionService.GetRoleByIdAsync(id);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Policy = "Permission:roles.create")]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var result = await _rolePermissionService.CreateRoleAsync(request);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:roles.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var result = await _rolePermissionService.UpdateRoleAsync(id, request);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:roles.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _rolePermissionService.DeleteRoleAsync(id);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize(Policy = "Permission:permissions.manage")]
    public async Task<IActionResult> SetPermissions(Guid id, [FromBody] List<Guid> permissionIds)
    {
        var result = await _rolePermissionService.SetRolePermissionsAsync(id, permissionIds);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Permissions updated successfully" });
    }
}
