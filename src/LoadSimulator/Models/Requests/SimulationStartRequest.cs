namespace LoadSimulator.Models.Requests;

/// <summary>
/// Request model to start simulation
/// </summary>
public class SimulationStartRequest
{
    public int Users { get; set; } = 100;
    
    public int? CartsPerUser { get; set; }
    
    public int? MaxProductsPerCart { get; set; }
    
    public int? DurationSeconds { get; set; }
}
