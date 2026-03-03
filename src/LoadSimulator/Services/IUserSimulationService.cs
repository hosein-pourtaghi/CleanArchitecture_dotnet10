namespace LoadSimulator.Services;

/// <summary>
/// Interface for simulating single user behavior
/// </summary>
public interface IUserSimulationService
{
    Task<UserSimulationResult> SimulateUserAsync(
        int userId,
        int cartsPerUser,
        int maxProductsPerCart,
        int delayMinMs,
        int delayMaxMs,
        double normalDistMean,
        double normalDistStdDev,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of single user simulation
/// </summary>
public class UserSimulationResult
{
    public int UserId { get; set; }
    public bool Success { get; set; }
    public int CartsCreated { get; set; }
    public int CartsFailed { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> Errors { get; set; } = new();
    public long TotalResponseTimeMs { get; set; }
}
