# LoadSimulator Advanced Configuration Guide

## Overview

This guide provides advanced configuration options for tuning the LoadSimulator for specific scenarios and workloads.

## Performance Tuning

### User Ramp-Up Configuration

Control how quickly virtual users are added to the simulation:

```json
{
  "Simulator": {
    "RampUpTimeSeconds": 60,  // Total time to reach full user load
    "ConcurrentUsers": 1000   // Target number of users
  }
}
```

With these settings:
- 1000 users will be added over 60 seconds
- Each user starts after ~60ms interval
- Realistic load growth pattern
- Prevents sudden spike on target API

### Think Time Distribution

Configure realistic user behavior patterns:

```json
{
  "Simulator": {
    "NormalDistributionMean": 1000,      // Average think time in ms
    "NormalDistributionStdDev": 300,     // Variation (bell curve)
    "DelayMinMs": 500,                   // Minimum delay (fallback)
    "DelayMaxMs": 2000                   // Maximum delay (fallback)
  }
}
```

**Normal Distribution Example**:
- Mean: 1000ms, StdDev: 300ms
- ~68% of users wait 700-1300ms
- ~95% of users wait 400-1600ms
- Simulates realistic user behavior

### Order Configuration

```json
{
  "Simulator": {
    "OrdersPerUser": 5,           // Orders each user creates
    "MaxProductsPerOrder": 3,     // Max items per order
    "DefaultPageSize": 50         // Product list page size
  }
}
```

## Resilience Configuration

### HTTP Client Timeouts

Edit `Program.cs`:

```csharp
.ConfigureHttpClient(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);  // Increase if needed
})
```

### Polly Policy Tuning

Modify `Infrastructure/PollyPolicies.cs`:

```csharp
// Retry: 5 retries instead of 3
.WaitAndRetryAsync(
    retryCount: 5,
    sleepDurationProvider: attempt => 
        TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 200)
)

// Circuit Breaker: More sensitive
.CircuitBreakerAsync(
    handledEventsAllowedBeforeBreaking: 3,
    durationOfBreak: TimeSpan.FromSeconds(60)
)
```

## Caching Configuration

### Redis Integration

Enable Redis for product caching:

```json
{
  "Redis": {
    "Enabled": true,
    "ConnectionString": "redis-server:6379"
  }
}
```

**Benefits**:
- Reduces load on target API
- Faster product retrieval
- More realistic cache hit ratios
- ~30-50% reduction in API calls

### Cache Settings

Modify `Services/ProductCacheService.cs`:

```csharp
private const int CacheDurationSeconds = 300;  // Change cache TTL
```

## Logging Configuration

### Serilog Advanced Configuration

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "LoadSimulator": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Ansi",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq-server:5341",
          "apiKey": "your-api-key",
          "batchPostingLimit": 100
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId",
      "WithEnvironmentUserName"
    ]
  }
}
```

### Database Logging

#### PostgreSQL

```json
{
  "Logging": {
    "Database": {
      "Enabled": true,
      "Type": "PostgreSQL",
      "ConnectionString": "Host=postgres;Port=5432;Username=logsdb;Password=secret;Database=logsdb;"
    }
  }
}
```

#### SQL Server

```json
{
  "Logging": {
    "Database": {
      "Enabled": true,
      "Type": "SqlServer",
      "ConnectionString": "Server=sql-server;Database=LoadSimulatorLogs;User Id=sa;Password=secret;"
    }
  }
}
```

## Health Check Configuration

```json
{
  "HealthChecks": {
    "Enabled": true,
    "Tags": ["live", "ready"]
  }
}
```

Endpoints:
- `GET /health` - Full health report
- `GET /api/health` - API health
- `GET /api/health/ready` - Readiness probe
- `GET /api/health/live` - Liveness probe

## Prometheus Metrics

### Scrape Configuration

```yaml
# prometheus.yml
scrape_configs:
  - job_name: 'load-simulator'
    static_configs:
      - targets: ['localhost:5001']
    metrics_path: '/metrics'
    scrape_interval: 15s
```

### Key Metrics

- `http_requests_received_total` - Total requests
- `http_request_duration_seconds` - Request latencies
- `process_cpu_seconds_total` - CPU usage
- `process_resident_memory_bytes` - Memory usage

## Memory Optimization

### Large-Scale Simulations (10,000+ users)

```json
{
  "Simulator": {
    "ConcurrentUsers": 10000,
    "RampUpTimeSeconds": 300,  // Longer ramp-up reduces peak memory
    "OrdersPerUser": 2         // Reduce orders to save memory
  }
}
```

Memory usage estimates:
- 100 users: ~150MB
- 1,000 users: ~600MB
- 10,000 users: ~6GB

### Running Multiple Instances

Use Docker to distribute load:

```yaml
version: '3'
services:
  simulator-1:
    image: load-simulator:latest
    environment:
      - Simulator__ConcurrentUsers=1000
      - Simulator__BaseUrl=http://api:5000
  
  simulator-2:
    image: load-simulator:latest
    environment:
      - Simulator__ConcurrentUsers=1000
      - Simulator__BaseUrl=http://api:5000
```

## Network Optimization

### HTTP/2 Multiplexing

Already enabled by default:

```csharp
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    EnableMultipleHttp2Connections = true,
    MaxConnectionsPerServer = 100
})
```

### Connection Pooling

Configure in `Program.cs`:

```csharp
new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
    MaxConnectionsPerServer = 200,  // Increase for high throughput
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
}
```

## Database Configuration

### Connection Pool Sizing

```json
{
  "ConnectionStrings": {
    "Default": "Server=...;Max Pool Size=100;Application Name=LoadSimulator;"
  }
}
```

## Monitoring Dashboard

### Grafana Dashboard JSON

Query examples:

```
# Requests per second
rate(http_requests_received_total[5m])

# Average response time
rate(http_request_duration_seconds_sum[5m]) / rate(http_request_duration_seconds_count[5m])

# Error rate
rate(http_requests_failed_total[5m])

# Memory usage
process_resident_memory_bytes / 1024 / 1024

# CPU usage
rate(process_cpu_seconds_total[5m]) * 100
```

## Troubleshooting

### High Memory Usage

1. Reduce `ConcurrentUsers`
2. Reduce `OrdersPerUser`
3. Increase `RampUpTimeSeconds`
4. Enable Redis caching

### Timeouts

1. Increase client timeout in `Program.cs`
2. Adjust Polly retry policy
3. Reduce `ConcurrentUsers`
4. Check target API capacity

### Circuit Breaker Trips

1. Reduce `ConcurrentUsers`
2. Increase `DelayMinMs` and `DelayMaxMs`
3. Check target API health
4. Verify network connectivity

### Token Expiration

Tokens expire by default. If seeing many 401 errors:
1. Check `TokenExpiryTime` in logs
2. Verify JWT signing key on both sides
3. Increase token expiration on target API

## Advanced Scenarios

### Load Testing Specific Endpoints

Modify `Services/UserSimulationService.cs`:

```csharp
// Focus on specific product categories
var productIds = new[] { 1, 2, 3, 4, 5 };  // Specific products

// Or specific order patterns
var maxPrice = 1000m;  // High-value orders only
```

### Soak Testing

```json
{
  "Simulator": {
    "ConcurrentUsers": 100,
    "OrdersPerUser": 1000,  // Run for hours
    "RampUpTimeSeconds": 30
  }
}
```

### Stress Testing

Gradually increase load:

```powershell
# Start with 100 users
curl -X POST http://localhost:5001/api/simulation/start -d '{"users":100}'

# Wait 1 minute, then increase
Start-Sleep -Seconds 60

# Add 200 more users
curl -X POST http://localhost:5001/api/simulation/start -d '{"users":300}'
```

### Spike Testing

Sudden load increase:

```powershell
curl -X POST http://localhost:5001/api/simulation/start -d '{"users":5000}'
```
