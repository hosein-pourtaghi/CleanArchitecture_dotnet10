# LoadSimulator Integration & Deployment Guide

## Quick Integration

### Add to Solution

The LoadSimulator project is already integrated into your solution at:
```
src/LoadSimulator/LoadSimulator.csproj
```

### Run Locally

1. **Start Main API** (port 5000):
```powershell
dotnet run --project src/WebApi/WebApi.csproj
```

2. **Start LoadSimulator** (port 5001):
```powershell
dotnet run --project src/LoadSimulator/LoadSimulator.csproj
```

3. **Start Simulation**:
```bash
curl -X POST http://localhost:5001/api/simulation/start \
  -H "Content-Type: application/json" \
  -d '{"users": 100}'
```

## Configuration for Your API

### Update appsettings.json

```json
{
  "Simulator": {
    "BaseUrl": "http://localhost:5000",  // Your API URL
    "ConcurrentUsers": 100,
    "OrdersPerUser": 5,
    "MaxProductsPerOrder": 3,
    "DelayMinMs": 500,
    "DelayMaxMs": 2000,
    "RampUpTimeSeconds": 60,
    "NormalDistributionMean": 1000,
    "NormalDistributionStdDev": 300,
    "DefaultPageSize": 50
  }
}
```

## API Endpoint Requirements

The LoadSimulator expects your main API to provide these endpoints:

### 1. Authentication Endpoints

**Register User**:
```
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "userName": "user123"
}

Response:
{
  "token": "eyJ...",
  "expiresIn": 3600
}
```

**Login User**:
```
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}

Response:
{
  "token": "eyJ...",
  "expiresIn": 3600
}
```

### 2. Product Endpoints

**Get Products**:
```
GET /api/products?page=1&pageSize=50
Authorization: Bearer {token}

Response:
[
  {
    "id": 1,
    "name": "Product Name",
    "price": 99.99,
    "stock": 100
  }
]
```

**Get Product By ID**:
```
GET /api/products/{id}
Authorization: Bearer {token}

Response:
{
  "id": 1,
  "name": "Product Name",
  "price": 99.99,
  "stock": 100
}
```

### 3. Order Endpoints

**Create Order**:
```
POST /api/orders
Authorization: Bearer {token}

Response:
{
  "id": 1,
  "items": [],
  "totalAmount": 0,
  "status": "Pending"
}
```

**Add Order Item**:
```
POST /api/orders/{orderId}/items
Authorization: Bearer {token}
Content-Type: application/json

{
  "productId": 1,
  "quantity": 2,
  "price": 99.99
}

Response:
{
  "success": true
}
```

**Submit Order**:
```
POST /api/orders/{orderId}/submit
Authorization: Bearer {token}

Response:
{
  "success": true,
  "id": 1
}
```

## Adapting LoadSimulator to Your API

### Custom Authentication Flow

If your API uses different authentication (OAuth, API Keys, etc.):

1. **Modify `Services/AuthClient.cs`**:

```csharp
public async Task<UserSessionDto?> LoginAsync(
    string email,
    string password,
    CancellationToken cancellationToken = default)
{
    // Your custom auth logic
    var oauth = new { 
        client_id = "simulator",
        grant_type = "password",
        username = email,
        password = password
    };
    
    var response = await _httpClient.PostAsync(
        "/oauth/token", 
        oauth.AsJsonContent(), 
        cancellationToken);
    
    // Parse your auth response
    var result = await response.DeserializeAsync<dynamic>(cancellationToken);
    
    return new UserSessionDto
    {
        JwtToken = result["access_token"],
        TokenExpiryTime = DateTime.UtcNow.AddSeconds(result["expires_in"])
    };
}
```

### Custom Order Flow

If your orders have different operations:

1. **Extend `Services/OrderClient.cs`**:

```csharp
public async Task<bool> ApplyDiscountAsync(
    int orderId,
    string discountCode,
    string jwtToken,
    CancellationToken cancellationToken = default)
{
    var request = new HttpRequestMessage(
        HttpMethod.Post, 
        $"/api/orders/{orderId}/discounts");
    
    request.AddBearerToken(jwtToken);
    request.Content = new { code = discountCode }.AsJsonContent();
    
    var response = await _httpClient.SendAsync(request, cancellationToken);
    return response.IsSuccessStatusCode;
}
```

2. **Update `Services/UserSimulationService.cs`**:

```csharp
// In SimulateUserAsync method:
var discountApplied = await _orderClient.ApplyDiscountAsync(
    order.Id,
    "SUMMER20",
    userSession.JwtToken,
    cancellationToken);
```

### Custom Data Generation

Modify `Utilities/MockDataGenerator.cs`:

```csharp
public string GenerateCompanyName()
{
    var companies = new[] { "Acme", "Tech Corp", "Global Inc" };
    return companies[Random.Shared.Next(companies.Length)] 
        + Random.Shared.Next(1000);
}

public string GenerateShippingAddress()
{
    var streets = new[] { "Main St", "Oak Ave", "Pine Rd" };
    return $"{Random.Shared.Next(100, 999)} {streets[Random.Shared.Next(streets.Length)]}";
}
```

## Deployment Options

### Docker Compose (Recommended)

```yaml
version: '3.8'

services:
  # Main API
  api:
    build:
      context: .
      dockerfile: src/WebApi/Dockerfile
    ports:
      - "5000:5000"
    environment:
      - ConnectionString=Server=db;...
      - Jwt:Key=${JWT_KEY}
    depends_on:
      - db
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # LoadSimulator
  simulator:
    build:
      context: .
      dockerfile: src/LoadSimulator/Dockerfile
    ports:
      - "5001:5001"
    environment:
      - Simulator__BaseUrl=http://api:5000
      - Simulator__ConcurrentUsers=100
      - Serilog__WriteTo__0__Args__serverUrl=http://seq:5341
    depends_on:
      - api
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5001/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Optional: Database
  db:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      - SA_PASSWORD=${DB_PASSWORD}
      - ACCEPT_EULA=Y
    ports:
      - "1433:1433"

  # Optional: Logging (Seq)
  seq:
    image: datalust/seq:latest
    ports:
      - "5341:80"
    environment:
      - ACCEPT_EULA=Y

  # Optional: Caching (Redis)
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
```

**Run:**
```bash
docker-compose up -d
```

### Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: load-simulator
  namespace: default
spec:
  replicas: 1
  selector:
    matchLabels:
      app: load-simulator
  template:
    metadata:
      labels:
        app: load-simulator
    spec:
      containers:
      - name: load-simulator
        image: load-simulator:latest
        ports:
        - containerPort: 5001
        env:
        - name: Simulator__BaseUrl
          value: "http://api-service:5000"
        - name: Simulator__ConcurrentUsers
          value: "100"
        livenessProbe:
          httpGet:
            path: /api/health/live
            port: 5001
          initialDelaySeconds: 10
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /api/health/ready
            port: 5001
          initialDelaySeconds: 5
          periodSeconds: 5
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "2Gi"
            cpu: "2000m"

---
apiVersion: v1
kind: Service
metadata:
  name: load-simulator-service
spec:
  selector:
    app: load-simulator
  ports:
  - protocol: TCP
    port: 5001
    targetPort: 5001
  type: LoadBalancer
```

**Deploy:**
```bash
kubectl apply -f load-simulator-deployment.yaml
```

### Azure Container Instances

```bash
az container create \
  --resource-group myResourceGroup \
  --name load-simulator \
  --image load-simulator:latest \
  --cpu 2 \
  --memory 4 \
  --environment-variables \
    Simulator__BaseUrl=http://myapi.azurewebsites.net \
    Simulator__ConcurrentUsers=100 \
  --ports 5001 \
  --protocol tcp
```

## Monitoring Integration

### Application Insights

Add to `Program.cs`:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Prometheus Metrics

Already included! Scrape endpoint at `/metrics`:

```yaml
# prometheus.yml
scrape_configs:
  - job_name: 'load-simulator'
    static_configs:
      - targets: ['localhost:5001']
    metrics_path: '/metrics'
```

### Grafana Dashboard

Import dashboard or create panels:

```sql
-- Orders per second
sum(rate(http_requests_received_total{job="load-simulator"}[5m]))

-- Average response time
rate(http_request_duration_seconds_sum[5m]) / rate(http_request_duration_seconds_count[5m])

-- Error rate
rate(http_requests_failed_total[5m]) / rate(http_requests_received_total[5m])

-- Memory usage
process_resident_memory_bytes / 1024 / 1024

-- CPU usage
rate(process_cpu_seconds_total[5m]) * 100
```

## CI/CD Integration

### GitHub Actions

```yaml
name: Load Test Pipeline

on:
  schedule:
    - cron: '0 2 * * *'  # Daily at 2 AM
  workflow_dispatch:

jobs:
  load-test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Build API
        run: dotnet build src/WebApi/WebApi.csproj
      
      - name: Build LoadSimulator
        run: dotnet build src/LoadSimulator/LoadSimulator.csproj
      
      - name: Start API
        run: |
          dotnet run --project src/WebApi/WebApi.csproj &
          sleep 10
      
      - name: Run Load Test
        run: |
          dotnet run --project src/LoadSimulator/LoadSimulator.csproj &
          sleep 5
          curl -X POST http://localhost:5001/api/simulation/start \
            -H "Content-Type: application/json" \
            -d '{"users": 100}'
      
      - name: Generate Report
        if: always()
        run: |
          curl http://localhost:5001/api/simulation/metrics > metrics.json
          
      - name: Upload Results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: load-test-results
          path: metrics.json
```

## Health Check Endpoints

The LoadSimulator provides health endpoints compatible with:
- Kubernetes liveness probes
- Kubernetes readiness probes
- Azure health checks
- Docker health checks

```
GET /health           - Full health report
GET /api/health       - API health
GET /api/health/ready - Readiness probe
GET /api/health/live  - Liveness probe
```

## Troubleshooting

### API Connection Issues

1. Verify API is running:
```bash
curl http://localhost:5000/health
```

2. Check configuration:
```json
{
  "Simulator": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

3. Check network:
```bash
# From LoadSimulator container
docker exec load-simulator ping api
```

### Authentication Failures

1. Verify endpoints match your API:
- `/api/auth/register` or `/api/auth/signup`?
- Response format correct?

2. Check token format:
- Bearer token in Authorization header?
- Token expires properly?

### High Error Rates

1. Check API capacity:
```bash
curl http://localhost:5000/api/simulation/metrics
```

2. Reduce load:
```json
{
  "Simulator": {
    "ConcurrentUsers": 50,
    "DelayMinMs": 1000,
    "DelayMaxMs": 3000
  }
}
```

3. Check database connections:
```bash
# SQL Server
SELECT COUNT(*) as TotalConnections FROM sys.dm_exec_sessions
```

## Performance Validation Checklist

- [ ] API responds correctly to auth requests
- [ ] JWT tokens expire properly
- [ ] Product endpoints return valid data
- [ ] Orders can be created and submitted
- [ ] No memory leaks observed
- [ ] Response times are acceptable
- [ ] Error rates are below threshold
- [ ] Metrics are collected correctly
- [ ] Health checks pass

## Support & Documentation

- **README.md** - Project overview
- **ADVANCED_CONFIGURATION.md** - Detailed configuration options
- **USAGE_EXAMPLES.md** - Practical examples
- **PERFORMANCE_GUIDE.md** - Benchmarking and optimization

## Next Steps

1. **Configure** your API endpoints
2. **Customize** data generation for your domain
3. **Test** with small user counts first
4. **Monitor** API performance metrics
5. **Scale** load gradually
6. **Analyze** results and bottlenecks
7. **Optimize** based on findings
