
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IRolePermissionService _rolePermissionService;

    public PermissionsController(IRolePermissionService rolePermissionService)
    {
        _rolePermissionService = rolePermissionService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await _rolePermissionService.GetAllPermissionsAsync();

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _rolePermissionService.GetPermissionByIdAsync(id);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreatePermissionRequest request)
    {
        var result = await _rolePermissionService.CreatePermissionAsync(
            request.Name, request.Description, request.Category);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePermissionRequest request)
    {
        var result = await _rolePermissionService.UpdatePermissionAsync(
            id, request.Name, request.Description, request.Category);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _rolePermissionService.DeletePermissionAsync(id);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }
}

// DTOs for Permissions Controller
public record CreatePermissionRequest(string Name, string Description, string Category);
public record UpdatePermissionRequest(string Name, string Description, string Category);
