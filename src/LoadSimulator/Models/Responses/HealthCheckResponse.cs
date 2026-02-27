namespace LoadSimulator.Models.Responses;

/// <summary>
/// Health check response model
/// </summary>
public class HealthCheckResponse
{
    public string Status { get; set; } = "Healthy";
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public Dictionary<string, object> Details { get; set; } = new();
}
