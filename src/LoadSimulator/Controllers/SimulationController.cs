using System.Diagnostics;
using LoadSimulator.Configuration;
using LoadSimulator.Infrastructure;
using LoadSimulator.Models.Requests;
using LoadSimulator.Models.Responses;
using LoadSimulator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LoadSimulator.Controllers;

/// <summary>
/// Controller for managing load simulation
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SimulationController : ControllerBase
{
    private readonly IUserSimulationService _simulationService;
    private readonly SimulationMetricsService _metricsService;
    private readonly ILogger<SimulationController> _logger;
    private readonly SimulatorSettings _settings;

    public SimulationController(
        IUserSimulationService simulationService,
        SimulationMetricsService metricsService,
        ILogger<SimulationController> logger,
        IOptions<SimulatorSettings> settings)
    {
        _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Start load simulation with specified number of concurrent users
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<SimulationSummary>> StartSimulation(
        [FromBody] SimulationStartRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Users <= 0 || request.Users > 100000)
        {
            return BadRequest("Users must be between 1 and 100000");
        }

        var startTime = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        _metricsService.Reset();

        var cartsPerUser = request.CartsPerUser ?? _settings.CartsPerUser;
        var maxProductsPerCart = request.MaxProductsPerCart ?? _settings.MaxProductsPerCart;

        _logger.LogInformation(
            "Starting simulation with {Users} users, {CartsPerUser} carts each",
            request.Users,
            cartsPerUser);

        try
        {
            // Create cancellation token with optional duration
            using var cts = request.DurationSeconds.HasValue
                ? new CancellationTokenSource(TimeSpan.FromSeconds(request.DurationSeconds.Value))
                : new CancellationTokenSource();

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

            // Calculate ramp-up delay per user
            var rampUpDelayMs = _settings.RampUpTimeSeconds > 0
                ? (int)(_settings.RampUpTimeSeconds * 1000 / request.Users)
                : 0;

            // Execute simulation tasks with ramp-up
            var tasks = new List<Task<UserSimulationResult>>();

            for (int i = 0; i < request.Users; i++)
            {
                var userId = i + 1;

                // Stagger user creation for ramp-up
                if (rampUpDelayMs > 0 && i > 0)
                {
                    await Task.Delay(rampUpDelayMs, cancellationToken).ConfigureAwait(false);
                }

                var task = _simulationService.SimulateUserAsync(
                    userId,
                    cartsPerUser,
                    maxProductsPerCart,
                    _settings.DelayMinMs,
                    _settings.DelayMaxMs,
                    _settings.NormalDistributionMean,
                    _settings.NormalDistributionStdDev,
                    linkedCts.Token);

                tasks.Add(task);

                // Use Parallel.ForEachAsync for high-throughput scenarios
                if (i % 100 == 0 && i > 0)
                {
                    _logger.LogInformation("Started {UsersStarted}/{TotalUsers} users", i, request.Users);
                }
            }

            // Wait for all user simulations to complete
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            stopwatch.Stop();

            // Aggregate metrics
            foreach (var result in results)
            {
                if (result.Success)
                {
                    _metricsService.RecordSuccessfulUser();
                }
                else
                {
                    _metricsService.RecordFailedUser();
                }

                _metricsService.RecordResponseTime(result.TotalResponseTimeMs);

                for (int i = 0; i < result.CartsCreated; i++)
                {
                    _metricsService.RecordSuccessfulCart();
                }

                for (int i = 0; i < result.CartsFailed; i++)
                {
                    _metricsService.RecordFailedCart();
                }

                foreach (var error in result.Errors)
                {
                    var errorType = error.Contains("Authentication") ? "Authentication" :
                                   error.Contains("cart") ? "CartOperation" :
                                   error.Contains("product") ? "ProductRetrieval" : "Unknown";
                    _metricsService.RecordError(errorType);
                }
            }

            var snapshot = _metricsService.GetSnapshot(stopwatch.Elapsed);

            var summary = new SimulationSummary
            {
                TotalUsers = request.Users,
                SuccessfulUsers = snapshot.SuccessfulUsers,
                FailedUsers = snapshot.FailedUsers,
                TotalCarts = snapshot.TotalCarts,
                SuccessfulCarts = snapshot.SuccessfulCarts,
                FailedCarts = snapshot.FailedCarts,
                Duration = stopwatch.Elapsed,
                CartsPerSecond = snapshot.CartsPerSecond,
                AverageResponseTimeMs = snapshot.AverageResponseTime,
                MinResponseTimeMs = snapshot.MinResponseTime,
                MaxResponseTimeMs = snapshot.MaxResponseTime,
                P95ResponseTimeMs = snapshot.P95ResponseTime,
                P99ResponseTimeMs = snapshot.P99ResponseTime,
                TotalErrors = snapshot.TotalErrors,
                ErrorsByType = snapshot.ErrorCounts,
                StartTime = startTime,
                EndTime = DateTime.UtcNow,
                Status = "Completed"
            };

            _logger.LogInformation(
                "Simulation completed: {SuccessfulCarts}/{TotalCarts} carts, {CartsPerSecond} carts/sec",
                summary.SuccessfulCarts,
                summary.TotalCarts,
                summary.CartsPerSecond);

            return Ok(summary);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            var snapshot = _metricsService.GetSnapshot(stopwatch.Elapsed);

            var summary = new SimulationSummary
            {
                TotalUsers = request.Users,
                Duration = stopwatch.Elapsed,
                StartTime = startTime,
                EndTime = DateTime.UtcNow,
                Status = "Cancelled"
            };

            _logger.LogWarning("Simulation cancelled after {Duration}ms", stopwatch.ElapsedMilliseconds);
            return StatusCode(400, summary);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Simulation failed");
            return StatusCode(500, new { error = ex.Message, duration = stopwatch.Elapsed });
        }
    }

    /// <summary>
    /// Get current metrics
    /// </summary>
    [HttpGet("metrics")]
    public ActionResult<object> GetMetrics()
    {
        try
        {
            var snapshot = _metricsService.GetSnapshot(TimeSpan.Zero);

            return Ok(new
            {
                totalCarts = snapshot.TotalCarts,
                successfulCarts = snapshot.SuccessfulCarts,
                failedCarts = snapshot.FailedCarts,
                cartsPerSecond = snapshot.CartsPerSecond,
                averageResponseTime = snapshot.AverageResponseTime,
                p95ResponseTime = snapshot.P95ResponseTime,
                p99ResponseTime = snapshot.P99ResponseTime,
                totalErrors = snapshot.TotalErrors,
                errorsByType = snapshot.ErrorCounts
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metrics");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Reset metrics
    /// </summary>
    [HttpPost("metrics/reset")]
    public ActionResult ResetMetrics()
    {
        try
        {
            _metricsService.Reset();
            return Ok(new { message = "Metrics reset" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting metrics");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
