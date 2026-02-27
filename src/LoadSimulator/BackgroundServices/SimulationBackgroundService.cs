using LoadSimulator.Configuration;
using LoadSimulator.Infrastructure;
using LoadSimulator.Services;
using Microsoft.Extensions.Options;

namespace LoadSimulator.BackgroundServices;

/// <summary>
/// Background service for running simulations automatically
/// </summary>
public class SimulationBackgroundService : BackgroundService
{
    private readonly IUserSimulationService _simulationService;
    private readonly SimulationMetricsService _metricsService;
    private readonly ILogger<SimulationBackgroundService> _logger;
    private readonly SimulatorSettings _settings;
    private readonly IServiceProvider _serviceProvider;

    public SimulationBackgroundService(
        IUserSimulationService simulationService,
        SimulationMetricsService metricsService,
        ILogger<SimulationBackgroundService> logger,
        IOptions<SimulatorSettings> settings,
        IServiceProvider serviceProvider)
    {
        _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Simulation Background Service starting");

        // Wait for application startup
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);

        // Run simulation loop if enabled
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation(
                    "Starting background simulation with {Users} users",
                    _settings.ConcurrentUsers);

                await RunSimulationAsync(stoppingToken).ConfigureAwait(false);

                // Wait before next iteration
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Background simulation cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background simulation");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("Simulation Background Service stopped");
    }

    private async Task RunSimulationAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        _metricsService.Reset();

        // Calculate ramp-up delay
        var rampUpDelayMs = _settings.RampUpTimeSeconds > 0
            ? (int)(_settings.RampUpTimeSeconds * 1000 / _settings.ConcurrentUsers)
            : 0;

        // Execute simulations
        var tasks = new List<Task<UserSimulationResult>>();

        for (int i = 0; i < _settings.ConcurrentUsers; i++)
        {
            var userId = i + 1;

            if (rampUpDelayMs > 0 && i > 0)
            {
                await Task.Delay(rampUpDelayMs, cancellationToken).ConfigureAwait(false);
            }

            var task = _simulationService.SimulateUserAsync(
                userId,
                _settings.OrdersPerUser,
                _settings.MaxProductsPerOrder,
                _settings.DelayMinMs,
                _settings.DelayMaxMs,
                _settings.NormalDistributionMean,
                _settings.NormalDistributionStdDev,
                cancellationToken);

            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        // Aggregate metrics
        var successCount = 0;
        var failureCount = 0;
        var totalOrders = 0;

        foreach (var result in results)
        {
            if (result.Success)
                successCount++;
            else
                failureCount++;

            totalOrders += result.OrdersCreated + result.OrdersFailed;
        }

        var duration = DateTime.UtcNow - startTime;

        _logger.LogInformation(
            "Background simulation completed: {SuccessCount}/{TotalUsers} successful, {TotalOrders} orders in {Duration}",
            successCount,
            _settings.ConcurrentUsers,
            totalOrders,
            duration);
    }
}
