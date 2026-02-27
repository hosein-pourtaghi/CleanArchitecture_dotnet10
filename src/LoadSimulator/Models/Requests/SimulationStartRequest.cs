namespace LoadSimulator.Models.Requests;

/// <summary>
/// Request model to start simulation
/// </summary>
public class SimulationStartRequest
{
    public int Users { get; set; } = 100;
    
    public int? OrdersPerUser { get; set; }
    
    public int? MaxProductsPerOrder { get; set; }
    
    public int? DurationSeconds { get; set; }
}
