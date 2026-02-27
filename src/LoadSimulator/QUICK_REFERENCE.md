# LoadSimulator - Quick Reference Card

## Start the Project

```bash
# Development
dotnet run --project src/LoadSimulator/LoadSimulator.csproj

# Production
dotnet publish -c Release -o ./publish src/LoadSimulator/LoadSimulator.csproj
dotnet publish/LoadSimulator.dll
```

## API Quick Commands

### Start Simulation
```bash
# 100 users, 5 orders each
curl -X POST http://localhost:5001/api/simulation/start \
  -H "Content-Type: application/json" \
  -d '{"users": 100}'

# 1000 users, custom settings
curl -X POST http://localhost:5001/api/simulation/start \
  -H "Content-Type: application/json" \
  -d '{
    "users": 1000,
    "ordersPerUser": 10,
    "maxProductsPerOrder": 5,
    "durationSeconds": 600
  }'
```

### Get Metrics
```bash
curl http://localhost:5001/api/simulation/metrics | jq
```

### Reset Metrics
```bash
curl -X POST http://localhost:5001/api/simulation/metrics/reset
```

### Health Check
```bash
curl http://localhost:5001/health | jq
curl http://localhost:5001/api/health/ready
curl http://localhost:5001/api/health/live
```

## Docker Commands

```bash
# Build
docker build -t load-simulator -f src/LoadSimulator/Dockerfile .

# Run standalone
docker run -p 5001:5001 \
  -e Simulator__BaseUrl=http://host.docker.internal:5000 \
  load-simulator

# Docker Compose
docker-compose up
docker-compose down
```

## Configuration

### Key Settings (appsettings.json)
```json
{
  "Simulator": {
    "BaseUrl": "http://localhost:5000",
    "ConcurrentUsers": 100,
    "OrdersPerUser": 5,
    "MaxProductsPerOrder": 3,
    "DelayMinMs": 500,
    "DelayMaxMs": 2000,
    "RampUpTimeSeconds": 60,
    "NormalDistributionMean": 1000,
    "NormalDistributionStdDev": 300
  },
  "Redis": {
    "Enabled": false,
    "ConnectionString": "localhost:6379"
  }
}
```

### Environment Variables
```bash
Simulator__BaseUrl=http://api.example.com
Simulator__ConcurrentUsers=500
Simulator__OrdersPerUser=10
Redis__Enabled=true
Redis__ConnectionString=redis:6379
```

## Code Customization

### Add Custom Authentication
Edit: `src/LoadSimulator/Services/AuthClient.cs`

### Add Custom User Flow
Edit: `src/LoadSimulator/Services/UserSimulationService.cs`

### Generate Custom Data
Edit: `src/LoadSimulator/Utilities/MockDataGenerator.cs`

### Tune Resilience Policies
Edit: `src/LoadSimulator/Infrastructure/PollyPolicies.cs`

## Testing Profiles

### Light (Dev Testing)
```bash
curl -X POST http://localhost:5001/api/simulation/start \
  -d '{"users": 10}'
```

### Normal (Functional Testing)
```bash
curl -X POST http://localhost:5001/api/simulation/start \
  -d '{"users": 100, "ordersPerUser": 5}'
```

### Peak (Performance Testing)
```bash
curl -X POST http://localhost:5001/api/simulation/start \
  -d '{"users": 1000, "ordersPerUser": 10}'
```

### Stress Test
```bash
curl -X POST http://localhost:5001/api/simulation/start \
  -d '{"users": 5000, "ordersPerUser": 20}'
```

## Monitoring

### Check Metrics
```bash
curl http://localhost:5001/api/simulation/metrics | \
  jq '{
    ordersPerSecond,
    averageResponseTime,
    failureRate: (.failedOrders / .totalOrders)
  }'
```

### Real-time Monitoring (Bash)
```bash
watch -n 5 'curl -s http://localhost:5001/api/simulation/metrics | jq'
```

### Real-time Monitoring (PowerShell)
```powershell
while ($true) {
  Clear-Host
  Invoke-RestMethod http://localhost:5001/api/simulation/metrics | Format-Table
  Start-Sleep -Seconds 5
}
```

## Troubleshooting

### API Not Reachable
```bash
curl http://your-api:5000/health
# If fails, check BaseUrl in appsettings.json
```

### High Error Rate
```bash
# Reduce concurrent users
# Increase delays
# Check API logs
```

### Out of Memory
```bash
# Reduce ConcurrentUsers
# Increase RampUpTimeSeconds
# Reduce OrdersPerUser
```

### Circuit Breaker Tripped
```bash
# Reset metrics
curl -X POST http://localhost:5001/api/simulation/metrics/reset

# Restart simulator
dotnet run --project src/LoadSimulator/LoadSimulator.csproj
```

## Response Example

```json
{
  "totalUsers": 100,
  "successfulUsers": 98,
  "failedUsers": 2,
  "totalOrders": 490,
  "successfulOrders": 485,
  "failedOrders": 5,
  "duration": "00:05:30.1234567",
  "ordersPerSecond": 1.47,
  "averageResponseTimeMs": 234.5,
  "minResponseTimeMs": 50,
  "maxResponseTimeMs": 2000,
  "p95ResponseTimeMs": 1800,
  "p99ResponseTimeMs": 1950,
  "totalErrors": 12,
  "errorsByType": {
    "OrderOperation": 5,
    "Authentication": 3,
    "ProductRetrieval": 4
  },
  "startTime": "2024-01-15T10:30:00Z",
  "endTime": "2024-01-15T10:35:30Z",
  "status": "Completed"
}
```

## Documentation Index

| Document | Use For |
|----------|---------|
| **README.md** | Overview and features |
| **PROJECT_SUMMARY.md** | What was created |
| **INTEGRATION_GUIDE.md** | Integration steps |
| **ADVANCED_CONFIGURATION.md** | Tuning and optimization |
| **USAGE_EXAMPLES.md** | Code examples |
| **PERFORMANCE_GUIDE.md** | Benchmarking |

## Common Tasks

### Simulate 1000 Users
```bash
curl -X POST http://localhost:5001/api/simulation/start \
  -H "Content-Type: application/json" \
  -d '{"users": 1000}'
```

### Get Current Performance
```bash
curl http://localhost:5001/api/simulation/metrics | jq .ordersPerSecond
```

### Run in Background
```bash
dotnet run --project src/LoadSimulator/LoadSimulator.csproj > simulator.log 2>&1 &
```

### Stop Simulator (if backgrounded)
```bash
pkill -f LoadSimulator
```

### Enable Redis Caching
```json
{
  "Redis": {
    "Enabled": true,
    "ConnectionString": "redis-server:6379"
  }
}
```

### Enable Database Logging
```json
{
  "Logging": {
    "Database": {
      "Enabled": true,
      "Type": "PostgreSQL",
      "ConnectionString": "Host=localhost;..."
    }
  }
}
```

## Performance Benchmarks

| Scenario | Users | Orders/sec | Avg Response |
|----------|-------|-----------|--------------|
| Light | 10 | 2-5 | 50-100ms |
| Normal | 100 | 20-50 | 150-300ms |
| Peak | 1000 | 200-500 | 300-1000ms |
| Stress | 5000 | 1000-2000 | 500-2000ms |

## Port Usage

- **API**: 5000 (main API)
- **LoadSimulator**: 5001
- **Redis** (optional): 6379
- **PostgreSQL** (optional): 5432
- **SQL Server** (optional): 1433
- **Seq** (logging, optional): 5341

## Environment Variables

```bash
# API URL
Simulator__BaseUrl=http://localhost:5000

# User Load
Simulator__ConcurrentUsers=100
Simulator__OrdersPerUser=5
Simulator__MaxProductsPerOrder=3

# Timing
Simulator__DelayMinMs=500
Simulator__DelayMaxMs=2000
Simulator__RampUpTimeSeconds=60
Simulator__NormalDistributionMean=1000
Simulator__NormalDistributionStdDev=300

# Redis
Redis__Enabled=true
Redis__ConnectionString=localhost:6379

# Logging
Serilog__MinimumLevel=Information
Logging__Database__Enabled=true
Logging__Database__Type=PostgreSQL
Logging__Database__ConnectionString=...
```

## Key Features Checklist

? Async/await throughout  
? HTTP/2 multiplexing  
? Connection pooling  
? Polly retry policy  
? Circuit breaker  
? Structured logging  
? Prometheus metrics  
? Health checks  
? Redis caching  
? Database logging  
? Docker support  
? Kubernetes ready  
? Background service  
? Normal distribution  
? Ramp-up control  
