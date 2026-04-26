using IdentityApi.Application.DTOs;
using IdentityApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers;

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
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await _rolePermissionService.GetAllRolesAsync();

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _rolePermissionService.GetRoleByIdAsync(id);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var result = await _rolePermissionService.CreateRoleAsync(request);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var result = await _rolePermissionService.UpdateRoleAsync(id, request);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _rolePermissionService.DeleteRoleAsync(id);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize]
    public async Task<IActionResult> SetPermissions(Guid id, [FromBody] List<Guid> permissionIds)
    {
        var result = await _rolePermissionService.SetRolePermissionsAsync(id, permissionIds);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Permissions updated successfully" });
    }
}
