using System.Diagnostics;
using Application.Abstractions.Interfaces;
using Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Extensions;

namespace Web.Api.Controllers;

/// <summary>
/// Authentication controller with OpenTelemetry instrumentation.
/// Logs all authentication operations with trace context and correlation IDs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth, ILogger<AuthController> logger) : ControllerBase
{
    private readonly IAuthService _auth = auth;

    /// <summary>
    /// Registers a new user.
    /// Logs the registration attempt and tracks execution time with OpenTelemetry.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var operationName = "AuthController.Register";
        var correlationId = HttpContext.TraceIdentifier;
        var stopwatch = Stopwatch.StartNew();

        using (var activity = logger.StartOperationSpan(operationName, correlationId))
        {
            try
            {
                activity?.SetTag("user.email", dto.Email);
                activity?.SetTag("operation.type", "registration");

                logger.LogInformation(
                    "User registration attempt. Email: {Email}, CorrelationId: {CorrelationId}",
                    dto.Email,
                    correlationId);

                string token = await _auth.RegisterAsync(dto);

                stopwatch.Stop();
                logger.LogOperationSuccess(operationName, stopwatch.ElapsedMilliseconds, correlationId);

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogOperationError(ex, operationName, correlationId);
                throw;
            }
        }
    }

    /// <summary>
    /// Authenticates a user and returns an authentication token.
    /// Logs the login attempt and tracks execution time with OpenTelemetry.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var operationName = "AuthController.Login";
        var correlationId = HttpContext.TraceIdentifier;
        var stopwatch = Stopwatch.StartNew();

        using (var activity = logger.StartOperationSpan(operationName, correlationId))
        {
            try
            {
                activity?.SetTag("user.email", dto.Email);
                activity?.SetTag("operation.type", "authentication");

                logger.LogInformation(
                    "User login attempt. Email: {Email}, CorrelationId: {CorrelationId}",
                    dto.Email,
                    correlationId);

                string token = await _auth.LoginAsync(dto);

                stopwatch.Stop();
                logger.LogOperationSuccess(operationName, stopwatch.ElapsedMilliseconds, correlationId);

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogOperationError(ex, operationName, correlationId);
                throw;
            }
        }
    }
}

