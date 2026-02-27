# LoadSimulator Usage Examples

## Quick Start

### 1. Basic Simulation - 100 Users

```bash
curl -X POST http://localhost:5001/api/simulation/start \
  -H "Content-Type: application/json" \
  -d '{"users": 100}'
```

### 2. Get Current Metrics

```bash
curl http://localhost:5001/api/simulation/metrics
```

### 3. Health Check

```bash
curl http://localhost:5001/health
```

## PowerShell Examples

### Run Simulation

```powershell
$params = @{
    Uri = "http://localhost:5001/api/simulation/start"
    Method = "POST"
    ContentType = "application/json"
    Body = @{
        users = 500
        ordersPerUser = 10
        maxProductsPerOrder = 5
    } | ConvertTo-Json
}

$result = Invoke-WebRequest @params
$result.Content | ConvertFrom-Json | Format-Table
```

### Monitor Metrics in Loop

```powershell
while ($true) {
    $metrics = Invoke-RestMethod -Uri "http://localhost:5001/api/simulation/metrics"
    Clear-Host
    Write-Host "=== Load Simulator Metrics ===" -ForegroundColor Green
    Write-Host "Total Orders: $($metrics.totalOrders)"
    Write-Host "Successful: $($metrics.successfulOrders)"
    Write-Host "Failed: $($metrics.failedOrders)"
    Write-Host "Orders/Second: $($metrics.ordersPerSecond)"
    Write-Host "Avg Response Time: $($metrics.averageResponseTime)ms"
    Write-Host "P95 Response Time: $($metrics.p95ResponseTime)ms"
    Write-Host "P99 Response Time: $($metrics.p99ResponseTime)ms"
    Write-Host "Errors: $($metrics.totalErrors)"
    Start-Sleep -Seconds 5
}
```

### Run Multiple Simulations Sequentially

```powershell
$configs = @(
    @{users = 100; ordersPerUser = 5},
    @{users = 500; ordersPerUser = 5},
    @{users = 1000; ordersPerUser = 5},
    @{users = 2000; ordersPerUser = 3}
)

foreach ($config in $configs) {
    Write-Host "Starting simulation with $($config.users) users..."
    
    $params = @{
        Uri = "http://localhost:5001/api/simulation/start"
        Method = "POST"
        ContentType = "application/json"
        Body = $config | ConvertTo-Json
    }
    
    $result = Invoke-WebRequest @params
    $summary = $result.Content | ConvertFrom-Json
    
    Write-Host "Completed: $($summary.successfulOrders)/$($summary.totalOrders) orders"
    Write-Host "Orders/sec: $($summary.ordersPerSecond)"
    Write-Host ""
    
    Start-Sleep -Seconds 30
}
```

## .NET Client Examples

### Using HttpClient

```csharp
using var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:5001");

// Start simulation
var request = new { users = 100 };
var json = JsonSerializer.Serialize(request);
var content = new StringContent(json, Encoding.UTF8, "application/json");

var response = await client.PostAsync("/api/simulation/start", content);
var result = await response.Content.ReadAsStringAsync();
var summary = JsonSerializer.Deserialize<SimulationSummary>(result);

Console.WriteLine($"Orders Created: {summary.SuccessfulOrders}");
Console.WriteLine($"Orders/Second: {summary.OrdersPerSecond}");
Console.WriteLine($"Avg Response Time: {summary.AverageResponseTimeMs}ms");
```

### Refit Client

```csharp
public interface ILoadSimulatorApi
{
    [Post("/api/simulation/start")]
    Task<SimulationSummary> StartSimulationAsync(
        SimulationStartRequest request);

    [Get("/api/simulation/metrics")]
    Task<MetricsResponse> GetMetricsAsync();

    [Post("/api/simulation/metrics/reset")]
    Task ResetMetricsAsync();

    [Get("/health")]
    Task<HealthCheckResponse> GetHealthAsync();
}

// Usage
var api = RestService.For<ILoadSimulatorApi>("http://localhost:5001");
var summary = await api.StartSimulationAsync(new { users = 100 });
```

## Bash Script Examples

### Load Test Script

```bash
#!/bin/bash

SIMULATOR_URL="http://localhost:5001"
OUTPUT_FILE="load_test_results.log"

echo "Starting Load Simulation Test Suite" | tee -a $OUTPUT_FILE
echo "=================================" | tee -a $OUTPUT_FILE

for users in 100 500 1000 5000; do
    echo ""
    echo "Running test with $users users..." | tee -a $OUTPUT_FILE
    
    response=$(curl -s -X POST "$SIMULATOR_URL/api/simulation/start" \
        -H "Content-Type: application/json" \
        -d "{\"users\": $users}")
    
    echo "$response" | jq . | tee -a $OUTPUT_FILE
    
    echo "Waiting 30 seconds before next test..." | tee -a $OUTPUT_FILE
    sleep 30
done

echo ""
echo "Load tests completed. Results saved to $OUTPUT_FILE"
```

### Real-time Monitoring Script

```bash
#!/bin/bash

SIMULATOR_URL="http://localhost:5001"

clear
while true; do
    echo "===== Load Simulator Metrics ====="
    echo "Timestamp: $(date)"
    echo ""
    
    curl -s "$SIMULATOR_URL/api/simulation/metrics" | jq '{
        totalOrders: .totalOrders,
        successfulOrders: .successfulOrders,
        failedOrders: .failedOrders,
        ordersPerSecond: .ordersPerSecond,
        avgResponseTime: .averageResponseTime,
        p95ResponseTime: .p95ResponseTime,
        p99ResponseTime: .p99ResponseTime,
        totalErrors: .totalErrors
    }'
    
    echo ""
    sleep 5
    clear
done
```

## Docker Examples

### Run with Docker Compose

```yaml
version: '3.8'

services:
  # Main API
  api:
    image: your-api:latest
    ports:
      - "5000:5000"
    environment:
      - ConnectionString=Server=db;...

  # LoadSimulator
  simulator:
    build:
      context: .
      dockerfile: Dockerfile.LoadSimulator
    ports:
      - "5001:5001"
    depends_on:
      - api
    environment:
      - Simulator__BaseUrl=http://api:5000
      - Simulator__ConcurrentUsers=100

  # Optional: Redis for caching
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  # Optional: Seq for logging
  seq:
    image: datalust/seq:latest
    ports:
      - "5341:80"
    environment:
      - ACCEPT_EULA=Y
```

### Run Simulator Container

```bash
docker run -d \
  --name load-simulator \
  -p 5001:5001 \
  -e Simulator__BaseUrl=http://host.docker.internal:5000 \
  -e Simulator__ConcurrentUsers=500 \
  load-simulator:latest
```

### Scale with Multiple Containers

```bash
docker-compose up -d --scale simulator=3
```

## Load Testing Profiles

### Light Load (Development Testing)

```json
{
  "users": 10,
  "ordersPerUser": 1,
  "maxProductsPerOrder": 1,
  "durationSeconds": 60
}
```

### Normal Load (Functional Testing)

```json
{
  "users": 100,
  "ordersPerUser": 5,
  "maxProductsPerOrder": 3,
  "durationSeconds": 300
}
```

### Peak Load (Performance Testing)

```json
{
  "users": 1000,
  "ordersPerUser": 10,
  "maxProductsPerOrder": 5
}
```

### Stress Test

```json
{
  "users": 5000,
  "ordersPerUser": 20,
  "maxProductsPerOrder": 3
}
```

### Soak Test (24-hour sustained load)

Configure in `appsettings.json`:

```json
{
  "Simulator": {
    "ConcurrentUsers": 100,
    "OrdersPerUser": 10000,
    "RampUpTimeSeconds": 60
  }
}
```

Then use Background Service which runs continuously.

## Analyzing Results

### Import to Excel

```powershell
$results = Invoke-RestMethod -Uri "http://localhost:5001/api/simulation/metrics"
$results | Export-Csv "metrics.csv" -NoTypeInformation
```

### Create Performance Report

```powershell
$summary = Invoke-RestMethod -Uri "http://localhost:5001/api/simulation/start" `
    -Method POST `
    -ContentType "application/json" `
    -Body '{"users": 100}'

$report = @"
=== Load Simulation Report ===
Date: $(Get-Date)
Duration: $($summary.duration)
Total Users: $($summary.totalUsers)
Successful Users: $($summary.successfulUsers)
Failed Users: $($summary.failedUsers)

Orders Summary:
- Total: $($summary.totalOrders)
- Successful: $($summary.successfulOrders)
- Failed: $($summary.failedOrders)
- Orders/sec: $($summary.ordersPerSecond)

Response Times (ms):
- Average: $($summary.averageResponseTimeMs)
- Min: $($summary.minResponseTimeMs)
- Max: $($summary.maxResponseTimeMs)
- P95: $($summary.p95ResponseTimeMs)
- P99: $($summary.p99ResponseTimeMs)

Errors: $($summary.totalErrors)
Error Types: $($summary.errorsByType | ConvertTo-Json)
"@

Write-Host $report
$report | Out-File "report_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
```

## Integration with CI/CD

### GitHub Actions

```yaml
name: Load Test

on:
  schedule:
    - cron: '0 0 * * *'  # Daily at midnight

jobs:
  load-test:
    runs-on: ubuntu-latest
    services:
      api:
        image: your-api:latest
        options: >-
          --health-cmd "curl -f http://localhost:5000/health || exit 1"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5000:5000

    steps:
      - uses: actions/checkout@v3
      
      - name: Build LoadSimulator
        run: docker build -t load-simulator -f src/LoadSimulator/Dockerfile .
      
      - name: Run Load Test
        run: |
          docker run --network host \
            -e Simulator__BaseUrl=http://localhost:5000 \
            load-simulator \
            --users 1000 --duration 300
      
      - name: Upload Results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: load-test-results
          path: results/
```

## Troubleshooting

### Check if Simulator is Running

```bash
curl -v http://localhost:5001/health
```

### View Logs

```bash
# Docker logs
docker logs load-simulator -f

# Application logs via Seq
curl http://localhost:5341/api/logs
```

### Reset Metrics

```bash
curl -X POST http://localhost:5001/api/simulation/metrics/reset
```

### Monitor Resource Usage

```powershell
while ($true) {
    $proc = Get-Process LoadSimulator -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "CPU: $($proc.CPU)% | Memory: $($proc.WorkingSet / 1MB)MB"
    }
    Start-Sleep -Seconds 1
}
```
