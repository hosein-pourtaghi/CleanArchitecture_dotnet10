# Swagger/OpenAPI Setup and Configuration

## Overview

This document describes the comprehensive Swagger/OpenAPI setup for the CleanArchitecture API, including JWT authentication integration, Swagger UI configuration, and how to access interactive API documentation.

---

## Quick Start

### Access Swagger UI

1. **Start the application**:
   ```bash
   cd src/Web.Api
   dotnet run
   ```

2. **Open Swagger UI** in your browser:
   ```
   http://localhost:5000/api-docs
   ```

3. **View OpenAPI specification**:
   ```
   http://localhost:5000/api-docs/v1/swagger.json
   ```

---

## Configuration Files

### 1. ServiceCollectionExtensions.cs

Located at: `src/Web.Api/Extensions/ServiceCollectionExtensions.cs`

This file configures Swagger generation with:

```csharp
services.AddSwaggerGen(options =>
{
    // API Information
    options.SwaggerDoc("v1", new()
    {
        Title = "CleanArchitecture API",
        Version = "v1.0",
        Description = "Comprehensive REST API for Customer Management System with JWT Authentication",
        Contact = new()
        {
            Name = "API Support",
            Email = "support@cleanarchitecture.local",
            Url = new Uri("https://github.com/yourusername/CleanArchitecture_dotnet10")
        },
        License = new()
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        },
        TermsOfService = new Uri("https://example.com/terms")
    });

    // XML Documentation from code comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});
```

**Features**:
- ✅ API metadata (title, version, description)
- ✅ Contact and license information
- ✅ XML documentation comment inclusion
- ✅ Custom schema IDs for generated types
- ✅ Endpoint sorting

### 2. ApplicationBuilderExtensions.cs

Located at: `src/Web.Api/Extensions/ApplicationBuilderExtensions.cs`

Configures Swagger UI and middleware:

```csharp
public static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
{
    // Enable Swagger middleware
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "api-docs/{documentName}/swagger.json";
    });

    // Enable Swagger UI
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/api-docs/v1/swagger.json", "CleanArchitecture API v1");
        options.RoutePrefix = "api-docs";
        
        // UI Features
        options.DocExpansion(DocExpansion.List);
        options.DefaultModelsExpandDepth(1);
        options.DefaultModelExpandDepth(1);
        options.DisplayOperationId();
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
        options.EnableFilter();
    });
}
```

**Features**:
- ✅ Custom Swagger endpoint route: `/api-docs/v1/swagger.json`
- ✅ Swagger UI at: `/api-docs`
- ✅ Try-it-out enabled by default
- ✅ Request duration display
- ✅ Filtering support
- ✅ Operation ID display

### 3. Program.cs

Located at: `src/Web.Api/Program.cs`

Integration point:

```csharp
// Add Swagger generation
builder.Services.AddSwaggerGenWithAuth();

// Enable Swagger UI in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();
}
```

---

## Endpoint Documentation

### Controllers with XML Documentation

All controllers have comprehensive XML documentation that appears in Swagger UI:

#### AuthController.cs
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login

#### CustomersController.cs
- `GET /api/customers` - Get all customers
- `GET /api/customers/{id}` - Get customer by ID
- `POST /api/customers` - Create customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer

#### HealthController.cs
- `GET /health` - Basic health check

### Response Type Attributes

Controllers use `[ProducesResponseType]` attributes to document all possible responses:

```csharp
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetCustomer(Guid id)
{
    // Implementation
}
```

---

## XML Documentation Comments

### File Generation

The Web.Api.csproj is configured to generate XML documentation:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591;1570</NoWarn>
</PropertyGroup>
```

This generates `Web.Api.xml` in the build output directory with:
- Class and method summaries
- Parameter descriptions
- Return value documentation
- Remarks and examples

### Example Documentation Format

```csharp
/// <summary>
/// Retrieves a customer by their unique identifier.
/// </summary>
/// <param name="id">The unique identifier of the customer to retrieve.</param>
/// <returns>
/// A customer with the specified ID.
/// Returns 200 OK on success with the customer data.
/// Returns 404 Not Found if no customer with the specified ID exists.
/// </returns>
/// <remarks>
/// This is a protected endpoint that requires JWT authentication.
/// The authenticated user can retrieve any customer's information.
/// </remarks>
public async Task<IActionResult> GetCustomer(Guid id)
{
    // Implementation
}
```

---

## Response Models Documentation

### RegisterResponse.cs

```csharp
/// <summary>
/// Response model for successful user registration.
/// Contains the JWT authentication token for immediate API access.
/// </summary>
public class RegisterResponse
{
    /// <summary>The JWT bearer token for authentication.</summary>
    public string Token { get; set; }

    /// <summary>Always "Bearer" for JWT authentication.</summary>
    public string TokenType { get; set; }

    /// <summary>Token expiration time in seconds.</summary>
    public int ExpiresIn { get; set; }

    /// <summary>Token expiration timestamp in UTC.</summary>
    public DateTime ExpiresAt { get; set; }
}
```

### LoginResponse.cs

```csharp
/// <summary>
/// Response model for successful user login.
/// Contains the JWT authentication token and user email.
/// </summary>
public class LoginResponse
{
    /// <summary>The JWT bearer token for authentication.</summary>
    public string Token { get; set; }

    /// <summary>Always "Bearer" for JWT authentication.</summary>
    public string TokenType { get; set; }

    /// <summary>Token expiration time in seconds.</summary>
    public int ExpiresIn { get; set; }

    /// <summary>Token expiration timestamp in UTC.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>The authenticated user's email address.</summary>
    public string Email { get; set; }
}
```

### ApiErrorResponse.cs

```csharp
/// <summary>
/// Standard error response format for all API errors.
/// Provides comprehensive error information for debugging.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>HTTP status code.</summary>
    public int Status { get; set; }

    /// <summary>Error title or type (e.g., "Validation Error").</summary>
    public string Title { get; set; }

    /// <summary>Detailed error description.</summary>
    public string Detail { get; set; }

    /// <summary>Unique trace ID for request tracking and logging.</summary>
    public string TraceId { get; set; }

    /// <summary>Field-level validation errors (if applicable).</summary>
    public Dictionary<string, string[]> Errors { get; set; }

    /// <summary>Response timestamp in UTC.</summary>
    public DateTime Timestamp { get; set; }
}
```

---

## Authentication in Swagger UI

### Adding JWT Token

1. **Click "Authorize" button** in Swagger UI top-right
2. **Enter token** with "Bearer " prefix:
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
3. **Click "Authorize"** to apply to all requests
4. **Click "Logout"** to clear the token

### Protected Endpoints

Endpoints requiring authentication show a **lock icon** 🔒 in Swagger UI:

- 🔒 `GET /api/customers` - Requires Bearer token
- 🔒 `POST /api/customers` - Requires Bearer token
- 🔒 `PUT /api/customers/{id}` - Requires Bearer token
- 🔒 `DELETE /api/customers/{id}` - Requires Bearer token

Public endpoints (no lock):
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /health`

---

## Swagger Customization

### Routes

| Route | Purpose |
|-------|---------|
| `/api-docs` | Swagger UI interface |
| `/api-docs/v1/swagger.json` | OpenAPI 3.0 specification (JSON) |

### UI Features Enabled

| Feature | Purpose |
|---------|---------|
| **Try It Out** | Test endpoints directly from UI |
| **Request Duration** | Shows how long each request takes |
| **Filter** | Search and filter endpoints |
| **Operation ID** | Display operation identifiers |
| **Doc Expansion** | Default expand level for operations |

---

## Using the OpenAPI Specification

### Download Specification

```bash
# Get the OpenAPI JSON spec
curl http://localhost:5000/api-docs/v1/swagger.json -o api-spec.json
```

### Convert to YAML

```bash
# Using swagger-cli
npx @apidevtools/swagger-cli bundle api-spec.json --outfile api-spec.yaml
```

### Generate Client Code

Using Swagger Code Generator:

```bash
# Generate C# client
swagger-codegen generate \
  -i http://localhost:5000/api-docs/v1/swagger.json \
  -l csharp \
  -o ./generated-client

# Generate TypeScript client
swagger-codegen generate \
  -i http://localhost:5000/api-docs/v1/swagger.json \
  -l typescript-axios \
  -o ./generated-client
```

---

## Swagger Endpoints Summary

### Development Environment

| Endpoint | Purpose | Access |
|----------|---------|--------|
| `http://localhost:5000/api-docs` | Interactive Swagger UI | Browser |
| `http://localhost:5000/api-docs/v1/swagger.json` | OpenAPI 3.0 JSON | Any client |

### Production Considerations

By default, Swagger UI is only enabled in Development environment:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();  // Only in Development
}
```

To enable in other environments:

```csharp
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwaggerWithUi();
}
```

---

## Best Practices

### 1. Keep Documentation Updated

Update XML comments whenever you:
- Add new endpoints
- Change request/response models
- Modify validation rules
- Add new error codes

### 2. Use Meaningful Summaries

```csharp
// ❌ Bad
/// <summary>Get customer.</summary>

// ✅ Good
/// <summary>
/// Retrieves a customer by their unique identifier.
/// This endpoint is used to fetch detailed information about a specific customer.
/// </summary>
```

### 3. Document All Status Codes

```csharp
/// <returns>
/// 200 OK: Customer found and returned.
/// 400 Bad Request: Invalid customer ID format.
/// 401 Unauthorized: Missing or invalid authentication token.
/// 404 Not Found: Customer not found in database.
/// </returns>
```

### 4. Include Examples

```csharp
/// <remarks>
/// Example request:
/// ```
/// GET /api/customers/550e8400-e29b-41d4-a716-446655440000
/// Authorization: Bearer {token}
/// ```
///
/// Example response:
/// ```json
/// {
///   "id": "550e8400-e29b-41d4-a716-446655440000",
///   "name": "John Doe",
///   "email": "john@example.com"
/// }
/// ```
/// </remarks>
```

---

## Testing Swagger Setup

### Verify OpenAPI Endpoint

```bash
# Check if Swagger JSON is accessible
curl -s http://localhost:5000/api-docs/v1/swagger.json | jq .info

# Output should show:
# {
#   "title": "CleanArchitecture API",
#   "version": "v1.0",
#   "description": "Comprehensive REST API..."
# }
```

### Validate OpenAPI Specification

```bash
# Using swagger-cli
npx @apidevtools/swagger-cli validate \
  http://localhost:5000/api-docs/v1/swagger.json
```

---

## Troubleshooting

### Swagger UI Not Appearing

1. **Check environment**:
   ```csharp
   // Only appears in Development
   if (app.Environment.IsDevelopment())
   {
       app.UseSwaggerWithUi();
   }
   ```

2. **Verify middleware order** - Swagger must be added after `UseRouting()`

3. **Check logs** for any errors during startup

### XML Comments Not Appearing

1. **Enable documentation generation** in .csproj:
   ```xml
   <GenerateDocumentationFile>true</GenerateDocumentationFile>
   ```

2. **Rebuild** the project:
   ```bash
   dotnet clean
   dotnet build
   ```

3. **Verify file exists**:
   ```bash
   # Should exist after build
   ls -la src/Web.Api/bin/Debug/net10.0/Web.Api.xml
   ```

### Authentication Not Working in Swagger

1. **Click "Authorize"** button in Swagger UI
2. **Enter valid JWT token** with "Bearer " prefix
3. **Verify token is valid** - check expiration time
4. **Check browser console** for CORS issues

---

## Related Documentation

- [API_USAGE_GUIDE.md](./API_USAGE_GUIDE.md) - Complete API usage guide with examples
- [CUSTOMER_CRUD_REFACTORING.md](./CUSTOMER_CRUD_REFACTORING.md) - CRUD implementation details
- [README.md](./README.md) - Project overview

---

**Last Updated**: January 15, 2024  
**Version**: 1.0  
**Status**: Production Ready
