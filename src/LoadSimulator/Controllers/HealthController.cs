using LoadSimulator.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LoadSimulator.Controllers;

/// <summary>
/// Health check controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check health status
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<HealthCheckResponse>> Check(CancellationToken cancellationToken)
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync(cancellationToken).ConfigureAwait(false);

            var response = new HealthCheckResponse
            {
                Status = report.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                Details = report.Entries
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => (object)new { status = kvp.Value.Status.ToString() })
            };

            var statusCode = report.Status == HealthStatus.Healthy ? 200 :
                           report.Status == HealthStatus.Degraded ? 200 : 503;

            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new HealthCheckResponse { Status = "Unhealthy" });
        }
    }

    /// <summary>
    /// Readiness probe
    /// </summary>
    [HttpGet("ready")]
    public ActionResult Ready()
    {
        return Ok(new { status = "ready" });
    }

    /// <summary>
    /// Liveness probe
    /// </summary>
    [HttpGet("live")]
    public ActionResult Live()
    {
        return Ok(new { status = "alive" });
    }
}
