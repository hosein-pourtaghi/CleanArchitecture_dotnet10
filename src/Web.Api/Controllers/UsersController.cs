using Application.Abstractions.Messaging;
using Application.Users.GetById;
using Application.Users.Login;
using Application.Users.Register;
using Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Web.Api.Controllers;

/// <summary>
/// Users API endpoints for authentication and user management.
/// </summary>
[Route("users")]
[Tags("Users")]
public class UsersController : ApiController
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        ICommandHandler<RegisterUserCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Password);

        var result = await handler.Handle(command, cancellationToken);
        return HandleCreatedResult(result, nameof(GetUserById), new { userId = result.Value });
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        ICommandHandler<LoginUserCommand, string> handler,
        CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request.Email, request.Password);
        var result = await handler.Handle(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves user information by ID.
    /// </summary>
    [HttpGet("{userId}")]
    [Authorize]
    [HasPermission(Permissions.UsersAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserById(
        Guid userId,
        IQueryHandler<GetUserByIdQuery, UserResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(userId);
        var result = await handler.Handle(query, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// Request model for user registration.
/// </summary>
public sealed record RegisterRequest(
    /// <summary>User email address.</summary>
    string Email,
    
    /// <summary>User's first name.</summary>
    string FirstName,
    
    /// <summary>User's last name.</summary>
    string LastName,
    
    /// <summary>User password (will be hashed).</summary>
    string Password);

/// <summary>
/// Request model for user login.
/// </summary>
public sealed record LoginRequest(
    /// <summary>User email address.</summary>
    string Email,
    
    /// <summary>User password.</summary>
    string Password);

/// <summary>
/// User permission constants.
/// </summary>
internal static class Permissions
{
    /// <summary>Permission to access users endpoints.</summary>
    internal const string UsersAccess = "users:access";
}
