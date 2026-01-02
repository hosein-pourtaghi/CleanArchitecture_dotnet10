using Application.Abstractions.Messaging;
using Application.Users.GetById;
using Application.Users.Login;
using Application.Users.Register;
using Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Controllers;

[ApiController]
[Route("users")]
[Tags("Users")]
public class UsersController : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
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

        Result<Guid> result = await handler.Handle(command, cancellationToken);
        return result.Match(Ok, CustomResults.Problem);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        LoginRequest request,
        ICommandHandler<LoginUserCommand, string> handler,
        CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request.Email, request.Password);
        Result<string> result = await handler.Handle(command, cancellationToken);
        return result.Match(Ok, CustomResults.Problem);
    }

    [HttpGet("{userId}")]
    [HasPermission(Permissions.UsersAccess)]
    public async Task<IActionResult> GetUserById(
        Guid userId,
        IQueryHandler<GetUserByIdQuery, UserResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(userId);
        Result<UserResponse> result = await handler.Handle(query, cancellationToken);
        return result.Match(Ok, CustomResults.Problem);
    }
}

public sealed record RegisterRequest(string Email, string FirstName, string LastName, string Password);

public sealed record LoginRequest(string Email, string Password);

internal static class Permissions
{
    internal const string UsersAccess = "users:access";
}
