using Application.Common.DTOs.Identities;
using Application.Common.Interfaces.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

/// <summary>
/// Authentication controller for user registration and login.
/// Provides JWT token-based authentication with comprehensive documentation and error handling.
/// </summary>
[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController(
    IAuthService _authService, 
    ICurrentUserService _currentUserService, 
    ILogger<AuthController> _logger
    ) : ControllerBase
{

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _authService.RegisterAsync(request, ipAddress, userAgent);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(Login), new { email = request.Email }, result.Value);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _authService.LoginAsync(request, ipAddress, userAgent);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var tokenId = User.FindFirst("jti")?.Value;
        var result = await _authService.LogoutAsync(_currentUserService.UserId, request, tokenId);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateToken([FromBody] string token)
    {
        var result = await _authService.ValidateTokenAsync(token);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        return Ok(new { valid = true });
    }
}
