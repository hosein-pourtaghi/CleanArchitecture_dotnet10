# Performance Benchmarks & Load Testing Guide

## Expected Performance Characteristics

Based on .NET 10 with async/await and connection pooling:

### Single Machine Capacity

| Metric | Value |
|--------|-------|
| Max Concurrent Users | 10,000+ |
| Max Orders/Second | 5,000+ |
| Memory per 100 Users | ~200-300 MB |
| CPU per 1000 Users | ~15-25% (4-core machine) |
| Avg Response Time | 50-500ms (depends on target API) |
| P99 Response Time | 1000-3000ms |

### Scaling Considerations

#### Vertical Scaling

```
1 Machine (16GB RAM, 8-core CPU):
??? 500 users ? ~2GB RAM
??? 1000 users ? ~3.5GB RAM
??? 5000 users ? ~12GB RAM
??? 10000 users ? ~20GB RAM (might exceed capacity)
```

#### Horizontal Scaling

```
3 Machines (each 8GB RAM, 4-core CPU):
??? Machine 1: 3,000 users ? ~6GB RAM
??? Machine 2: 3,000 users ? ~6GB RAM
??? Machine 3: 3,000 users ? ~6GB RAM
??? Total: 9,000 users across cluster
```

## Benchmarking Methodology

### 1. Baseline Test

Establish performance baseline on target API:

```powershell
$baseline = @{
    users = 10
    ordersPerUser = 1
}

$result = Invoke-RestMethod -Uri "http://localhost:5001/api/simulation/start" `
    -Method POST `
    -ContentType "application/json" `
    -Body ($baseline | ConvertTo-Json)

Write-Host "Baseline Orders/sec: $($result.ordersPerSecond)"
Write-Host "Baseline Response Time: $($result.averageResponseTimeMs)ms"
```

### 2. Ramp-up Test

Gradually increase load:

```powershell
$userCounts = @(10, 50, 100, 500, 1000, 5000)

$results = @()

foreach ($users in $userCounts) {
    Write-Host "Testing with $users users..."
    
    $result = Invoke-RestMethod -Uri "http://localhost:5001/api/simulation/start" `
        -Method POST `
        -ContentType "application/json" `
        -Body (@{users = $users} | ConvertTo-Json)
    
    $results += [PSCustomObject]@{
        Users = $users
        OrdersPerSec = $result.ordersPerSecond
        AvgResponseTime = $result.averageResponseTimeMs
        P99ResponseTime = $result.p99ResponseTimeMs
        FailureRate = ($result.failedOrders / $result.totalOrders)
    }
    
    Start-Sleep -Seconds 30
}

$results | Format-Table -AutoSize
```

### 3. Stress Test

Push until breaking point:

```powershell
$stressLevels = @(5000, 10000, 15000)

foreach ($users in $stressLevels) {
    Write-Host "Stress testing with $users users..."
    
    $result = Invoke-RestMethod -Uri "http://localhost:5001/api/simulation/start" `
        -Method POST `
        -ContentType "application/json" `
        -Body (@{users = $users} | ConvertTo-Json)
    
    Write-Host "Status: $($result.status)"
    Write-Host "Orders/sec: $($result.ordersPerSecond)"
    Write-Host "Failure Rate: $($result.failedOrders / $result.totalOrders * 100)%"
    
    if ($result.failedOrders / $result.totalOrders -gt 0.05) {
        Write-Host "Failure threshold (5%) exceeded. Stopping."
        break
    }
    
    Start-Sleep -Seconds 30
}
```

### 4. Soak Test

Sustained load over time:

```powershell
# Run for 6 hours
$startTime = Get-Date
$duration = New-TimeSpan -Hours 6

$metricsHistory = @()

while ((Get-Date) - $startTime -lt $duration) {
    $metrics = Invoke-RestMethod -Uri "http://localhost:5001/api/simulation/metrics"
    
    $metricsHistory += [PSCustomObject]@{
        Timestamp = Get-Date
        OrdersPerSec = $metrics.ordersPerSecond
        AvgResponseTime = $metrics.averageResponseTime
        FailureCount = $metrics.failedOrders
    }
    
    Start-Sleep -Seconds 60
}

# Analyze trends
$metricsHistory | 
    Group-Object { [math]::Floor([int]$_.Timestamp.TotalMinutes / 30) } |
    ForEach-Object { 
        $group = $_.Group
        [PSCustomObject]@{
            TimeWindow = $_.Name
            AvgOrdersPerSec = ($group.OrdersPerSec | Measure-Object -Average).Average
            MaxResponseTime = ($group.AvgResponseTime | Measure-Object -Maximum).Maximum
            TotalErrors = ($group.FailureCount | Measure-Object -Sum).Sum
        }
    } |
    Format-Table -AutoSize
```

## Performance Optimization Tips

### 1. Connection Pooling

```csharp
// Already configured, but can be tuned:
new SocketsHttpHandler
{
    MaxConnectionsPerServer = 200,  // Increase for higher throughput
    PooledConnectionLifetime = TimeSpan.FromMinutes(5)
}
```

### 2. Request Batching

Modify `UserSimulationService.cs`:

```csharp
// Instead of sequential requests:
for (int i = 0; i < itemCount; i++)
{
    await _orderClient.AddOrderItemAsync(...);
    await DelayAsync(...);
}

// Consider parallel requests:
var itemTasks = selectedProductIds.Select(productId =>
    _orderClient.AddOrderItemAsync(
        order.Id, productId, quantity, price, jwtToken, cancellationToken)
).ToList();

await Task.WhenAll(itemTasks);
```

### 3. Memory Optimization

```json
{
  "Simulator": {
    "RampUpTimeSeconds": 300,  // Slower ramp-up = lower peak memory
    "ConcurrentUsers": 5000,
    "OrdersPerUser": 2         // Fewer orders = less memory
  }
}
```

### 4. Disable Unnecessary Logging

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

## Monitoring During Tests

### CPU Usage

```powershell
# Monitor LoadSimulator process
Get-Counter -Counter "\Process(LoadSimulator)\% Processor Time" -SampleInterval 1 -MaxSamples 300
```

### Memory Usage

```powershell
# Real-time memory monitoring
while ($true) {
    $mem = (Get-Process LoadSimulator).WorkingSet / 1MB
    Write-Host "Memory: $([math]::Round($mem)) MB"
    Start-Sleep -Seconds 1
}
```

### Network I/O

```powershell
# Monitor network
Get-Counter -Counter "\NetworkInterface(*)\Bytes Received/sec" -SampleInterval 1
```

## Creating Performance Reports

### Automated Report Generation

```powershell
$testResults = @()

foreach ($scenario in @("Light", "Normal", "Peak")) {
    $users = @{
        "Light" = 100
        "Normal" = 500
        "Peak" = 2000
    }[$scenario]
    
    $result = Invoke-RestMethod -Uri "http://localhost:5001/api/simulation/start" `
        -Method POST `
        -ContentType "application/json" `
        -Body (@{users = $users} | ConvertTo-Json)
    
    $testResults += [PSCustomObject]@{
        Scenario = $scenario
        Users = $users
        "Orders/sec" = [math]::Round($result.ordersPerSecond, 2)
        "Avg Response" = [math]::Round($result.averageResponseTimeMs, 0)
        "P99 Response" = [math]::Round($result.p99ResponseTimeMs, 0)
        "Success Rate" = [math]::Round(($result.successfulOrders / $result.totalOrders) * 100, 2)
    }
}

$html = $testResults | ConvertTo-Html -As Table -Fragment
$html | Out-File "performance_report.html"
```

## Comparing Results

### Before/After Optimization

```powershell
# Store baseline
$baseline = @{
    ordersPerSec = 100
    avgResponseTime = 200
    failureRate = 0.01
}

# Test after optimization
$optimized = Invoke-RestMethod -Uri "http://localhost:5001/api/simulation/start" `
    -Method POST `
    -ContentType "application/json" `
    -Body (@{users = 500} | ConvertTo-Json)

# Calculate improvements
Write-Host "Performance Improvement:"
Write-Host "Orders/sec: $(($optimized.ordersPerSecond / $baseline.ordersPerSec * 100).ToString("F1"))%"
Write-Host "Response Time: $(($baseline.avgResponseTime / $optimized.avgResponseTime * 100).ToString("F1"))%"
Write-Host "Failure Rate Reduction: $(($baseline.failureRate - $optimized.failedOrders/$optimized.totalOrders).ToString("P"))"
```

## Load Test Checklist

- [ ] Baseline performance captured
- [ ] Ramp-up profile defined
- [ ] Stress test completed
- [ ] Failure modes identified
- [ ] Bottlenecks documented
- [ ] Optimization recommendations made
- [ ] Results reported to stakeholders
- [ ] Regression tests scheduled
