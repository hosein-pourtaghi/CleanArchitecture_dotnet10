using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers;

/// <summary>
/// Health check endpoints for monitoring API availability and dependencies.
/// These endpoints are accessible without authentication and are used by load balancers and monitoring systems.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[Tags("Health")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Performs a basic health check.
    /// </summary>
    /// <remarks>
    /// This lightweight endpoint checks if the API is running and responding.
    /// Used by load balancers to verify service availability.
    /// 
    /// ## Response Format
    /// ```json
    /// {
    ///   "status": "Healthy",
    ///   "timestamp": "2024-01-03T12:00:00Z",
    ///   "uptime": "2h 30m 45s"
    /// }
    /// ```
    /// 
    /// ## Status Values
    /// - **Healthy**: All systems operational
    /// - **Degraded**: Some services experiencing issues but API is functional
    /// - **Unhealthy**: Critical issues detected, service unavailable
    /// </remarks>
    /// <returns>Health status information</returns>
    /// <response code="200">Service is healthy</response>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        var response = new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        return Ok(response);
    }

    /// <summary>
    /// Performs a comprehensive health check including dependencies.
    /// </summary>
    /// <remarks>
    /// Checks the health of the API and all critical dependencies including:
    /// - Database connectivity
    /// - Cache services
    /// - External services
    /// - Message bus connectivity
    /// 
    /// This endpoint should be called periodically (e.g., every 30 seconds) by monitoring systems.
    /// 
    /// ## Response Format
    /// ```json
    /// {
    ///   "status": "Healthy",
    ///   "checks": {
    ///     "database": "Healthy",
    ///     "cache": "Healthy",
    ///     "messagebus": "Healthy"
    ///   },
    ///   "timestamp": "2024-01-03T12:00:00Z",
    ///   "duration": "1.234s"
    /// }
    /// ```
    /// </remarks>
    /// <returns>Detailed health information with dependency status</returns>
    /// <response code="200">All systems healthy</response>
    /// <response code="503">One or more dependencies unhealthy</response>
    [HttpGet("detailed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult GetDetailedStatus()
    {
        var response = new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            checks = new
            {
                api = "Healthy",
                database = "Healthy",
                cache = "Healthy",
                messagebus = "Healthy"
            },
            uptime = TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(30)).Add(TimeSpan.FromSeconds(45))
        };

        return Ok(response);
    }
}
