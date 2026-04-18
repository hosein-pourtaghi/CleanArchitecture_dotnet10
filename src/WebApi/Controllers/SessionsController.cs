using Application.Common.Interfaces.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController(ISessionService _sessionService, ICurrentUserService _userContext) : ControllerBase
{

    [HttpGet("online")]
    [Authorize]
    public async Task<IActionResult> GetOnlineUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _sessionService.GetOnlineUsersAsync(page, pageSize);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("my-sessions")]
    public async Task<IActionResult> GetMySessions()
    {
        var result = await _sessionService.GetUserSessionsAsync(_userContext.UserId);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpDelete("{sessionId:guid}")]
    [Authorize]
    public async Task<IActionResult> TerminateSession(Guid sessionId)
    {
        var result = await _sessionService.TerminateSessionAsync(_userContext.UserId, sessionId);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Session terminated successfully" });
    }

    [HttpDelete("terminate-all")]
    [Authorize]
    public async Task<IActionResult> TerminateAllSessions([FromQuery] Guid? userId)
    {
        var targetUserId = userId ?? _userContext.UserId;
        var result = await _sessionService.TerminateAllSessionsAsync(targetUserId, null);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "All sessions terminated successfully" });
    }
}
