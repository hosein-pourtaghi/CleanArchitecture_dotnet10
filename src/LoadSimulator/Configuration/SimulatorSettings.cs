namespace LoadSimulator.Configuration;

/// <summary>
/// Configuration settings for load simulation
/// </summary>
public class SimulatorSettings
{
    public string BaseUrl { get; set; } = "http://localhost:5000";
    
    public int ConcurrentUsers { get; set; } = 100;
    
    public int OrdersPerUser { get; set; } = 5;
    
    public int MaxProductsPerOrder { get; set; } = 3;
    
    public int DelayMinMs { get; set; } = 500;
    
    public int DelayMaxMs { get; set; } = 2000;
    
    public int RampUpTimeSeconds { get; set; } = 60;
    
    public double NormalDistributionMean { get; set; } = 1000;
    
    public double NormalDistributionStdDev { get; set; } = 300;
    
    public int DefaultPageSize { get; set; } = 50;
}

public class PrometheusSettings
{
    public string MetricsPath { get; set; } = "/metrics";
    public bool Enabled { get; set; } = true;
}
