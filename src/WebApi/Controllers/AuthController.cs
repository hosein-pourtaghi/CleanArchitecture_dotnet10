using System.Diagnostics;
using Application.Abstractions.Interfaces;
using Application.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;
using WebApi.Models.Authentication;

namespace WebApi.Controllers;

/// <summary>
/// Authentication controller for user registration and login.
/// Provides JWT token-based authentication with comprehensive documentation and error handling.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Authentication")]
[Produces("application/json")]
[Consumes("application/json")]
public class AuthController(IAuthService auth, ILogger<AuthController> logger) : ControllerBase
{
    private readonly IAuthService _auth = auth;

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <remarks>
    /// ## Authentication Flow
    /// 1. User provides email and password
    /// 2. System validates input and checks for existing account
    /// 3. Password is hashed and stored securely
    /// 4. JWT token is generated with user claims
    /// 5. Token is returned to client for immediate API access
    /// 
    /// ## Token Usage
    /// Include the returned token in the Authorization header for subsequent requests:
    /// ```
    /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    /// ```
    /// 
    /// ## Password Requirements
    /// - Minimum 8 characters
    /// - At least one uppercase letter
    /// - At least one lowercase letter
    /// - At least one digit
    /// - At least one special character
    /// 
    /// ## Error Handling
    /// - 400: Invalid email format or weak password
    /// - 409: Email already registered
    /// - 500: Server error
    /// </remarks>
    /// <param name="dto">Registration credentials including email and password</param>
    /// <returns>JWT token with user details</returns>
    /// <response code="200">Registration successful, JWT token returned</response>
    /// <response code="400">Invalid input - validation error</response>
    /// <response code="409">Email already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
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
                var expiresAt = DateTime.UtcNow.AddHours(24); // Assuming 24-hour token validity

                stopwatch.Stop();
                logger.LogOperationSuccess(operationName, stopwatch.ElapsedMilliseconds, correlationId);

                var response = new RegisterResponse
                {
                    Token = token,
                    TokenType = "Bearer",
                    ExpiresIn = 86400, // 24 hours in seconds
                    ExpiresAt = expiresAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogOperationError(ex, operationName, correlationId);
                
                var errorResponse = new ApiErrorResponse
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Registration Failed",
                    Detail = ex.Message,
                    TraceId = correlationId
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <remarks>
    /// ## Authentication Flow
    /// 1. User provides email and password
    /// 2. System validates credentials against stored user
    /// 3. Password is compared with securely stored hash
    /// 4. If valid, JWT token is generated with user claims
    /// 5. Token is returned to client for API access
    /// 
    /// ## Token Format
    /// JWT (JSON Web Token) structure:
    /// - **Header**: Contains algorithm and token type
    /// - **Payload**: Contains user claims (email, roles, permissions)
    /// - **Signature**: Validates token integrity
    /// 
    /// ## Token Lifetime
    /// - **Access Token**: 24 hours (configurable)
    /// - Tokens cannot be revoked; user must wait for expiration
    /// - Request a new token before expiration
    /// 
    /// ## Usage Example
    /// ```bash
    /// # 1. Get token
    /// curl -X POST https://api.example.com/api/auth/login \
    ///   -H "Content-Type: application/json" \
    ///   -d '{"email":"user@example.com","password":"SecurePass123!"}'
    /// 
    /// # 2. Use token in requests
    /// curl https://api.example.com/api/customers \
    ///   -H "Authorization: Bearer {token}"
    /// ```
    /// 
    /// ## Error Responses
    /// - 401: Invalid credentials
    /// - 404: User not found
    /// - 500: Server error
    /// </remarks>
    /// <param name="dto">Login credentials (email and password)</param>
    /// <returns>JWT token with user email</returns>
    /// <response code="200">Login successful, JWT token returned</response>
    /// <response code="401">Invalid email or password</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
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
                var expiresAt = DateTime.UtcNow.AddHours(24); // Assuming 24-hour token validity

                stopwatch.Stop();
                logger.LogOperationSuccess(operationName, stopwatch.ElapsedMilliseconds, correlationId);

                var response = new LoginResponse
                {
                    Token = token,
                    TokenType = "Bearer",
                    ExpiresIn = 86400, // 24 hours in seconds
                    ExpiresAt = expiresAt,
                    Email = dto.Email
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogOperationError(ex, operationName, correlationId);
                
                var errorResponse = new ApiErrorResponse
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Login Failed",
                    Detail = ex.Message,
                    TraceId = correlationId
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}

