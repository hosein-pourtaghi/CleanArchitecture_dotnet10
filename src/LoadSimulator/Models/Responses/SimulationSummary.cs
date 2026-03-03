namespace LoadSimulator.Models.Responses;

/// <summary>
/// Summary of load simulation execution
/// </summary>
public class SimulationSummary
{
    public int TotalUsers { get; set; }
    
    public int SuccessfulUsers { get; set; }
    
    public int FailedUsers { get; set; }
    
    public long TotalCarts { get; set; }
    
    public long SuccessfulCarts { get; set; }
    
    public long FailedCarts { get; set; }
    
    public TimeSpan Duration { get; set; }
    
    public double CartsPerSecond { get; set; }
    
    public double AverageResponseTimeMs { get; set; }
    
    public double MinResponseTimeMs { get; set; }
    
    public double MaxResponseTimeMs { get; set; }
    
    public double P95ResponseTimeMs { get; set; }
    
    public double P99ResponseTimeMs { get; set; }
    
    public int TotalErrors { get; set; }
    
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    
    public DateTime StartTime { get; set; }
    
    public DateTime EndTime { get; set; }
    
    public string Status { get; set; } = "Completed";
}
