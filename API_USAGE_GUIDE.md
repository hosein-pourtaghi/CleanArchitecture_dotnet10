# CleanArchitecture API - Complete Usage Guide

## Table of Contents
1. [API Overview](#api-overview)
2. [Authentication Flow](#authentication-flow)
3. [API Endpoints](#api-endpoints)
4. [Error Handling](#error-handling)
5. [Examples](#examples)
6. [Rate Limiting](#rate-limiting)
7. [Testing](#testing)

---

## API Overview

### Base URL
```
http://localhost:5000/api
https://yourserver.com/api
```

### Swagger/OpenAPI Documentation
Interactive API documentation is available at:
- **Swagger UI**: `http://localhost:5000/api-docs`
- **OpenAPI JSON**: `http://localhost:5000/api-docs/v1/swagger.json`

The Swagger UI provides:
- ✅ Interactive endpoint testing
- ✅ Real-time JWT token authentication
- ✅ Request/response examples
- ✅ Complete parameter documentation
- ✅ Response schema definitions

---

## Authentication Flow

### Overview
The API uses JWT (JSON Web Tokens) for stateless authentication with role-based access control.

### Token Type
- **Scheme**: Bearer
- **Format**: JWT (JSON Web Token)
- **Expiration**: Configurable (default: 1 hour)
- **Algorithm**: HS256 (Symmetric signing)

### Step 1: Register a New Account

**Endpoint**: `POST /api/auth/register`

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "expiresAt": "2024-01-15T15:30:00Z"
}
```

**Error Responses**:
- `400 Bad Request`: Invalid input or password requirements not met
- `409 Conflict`: Email already registered

### Step 2: Login with Existing Account

**Endpoint**: `POST /api/auth/login`

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "expiresAt": "2024-01-15T15:30:00Z",
  "email": "user@example.com"
}
```

**Error Responses**:
- `400 Bad Request`: Invalid credentials
- `404 Not Found`: User not found

### Step 3: Use Token in Protected Endpoints

All authenticated endpoints require the JWT token in the `Authorization` header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Claims
The JWT token contains:
```json
{
  "sub": "user@example.com",
  "email": "user@example.com",
  "role": "User",
  "iat": 1705334400,
  "exp": 1705338000,
  "iss": "CleanArchitecture"
}
```

---

## API Endpoints

### Health Checks

#### Get Health Status
**Endpoint**: `GET /health`
**Authentication**: None (Anonymous)
**Description**: Returns basic health status of the application

**Response** (200 OK):
```json
{
  "status": "Healthy"
}
```

---

### Authentication Endpoints

#### Register User
**Endpoint**: `POST /api/auth/register`
**Authentication**: None (Anonymous)
**Description**: Create a new user account

**Request**:
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "Password123!",
    "firstName": "Jane",
    "lastName": "Smith"
  }'
```

**Success Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "expiresAt": "2024-01-15T15:30:00Z"
}
```

---

#### Login User
**Endpoint**: `POST /api/auth/login`
**Authentication**: None (Anonymous)
**Description**: Authenticate user and receive JWT token

**Request**:
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

**Success Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "expiresAt": "2024-01-15T15:30:00Z",
  "email": "user@example.com"
}
```

---

### Customer Management Endpoints

All customer endpoints require JWT authentication.

#### Get All Customers
**Endpoint**: `GET /api/customers`
**Authentication**: Required (Bearer Token)
**Description**: Retrieve list of all customers

**Request**:
```bash
curl -X GET "http://localhost:5000/api/customers" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Response** (200 OK):
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "+1-555-0001",
    "address": "123 Main St, Springfield, IL 62701",
    "createdAt": "2024-01-10T10:00:00Z",
    "updatedAt": "2024-01-10T10:00:00Z"
  }
]
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid token

---

#### Get Customer by ID
**Endpoint**: `GET /api/customers/{id}`
**Authentication**: Required (Bearer Token)
**Description**: Retrieve a specific customer by ID

**Request**:
```bash
curl -X GET "http://localhost:5000/api/customers/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "+1-555-0001",
  "address": "123 Main St, Springfield, IL 62701",
  "createdAt": "2024-01-10T10:00:00Z",
  "updatedAt": "2024-01-10T10:00:00Z"
}
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid token
- `404 Not Found`: Customer not found

---

#### Create Customer
**Endpoint**: `POST /api/customers`
**Authentication**: Required (Bearer Token)
**Description**: Create a new customer

**Validation Rules**:
- Name: Required, 2-100 characters
- Email: Required, valid email format, must be unique
- Phone: Optional, valid phone format
- Address: Optional, max 500 characters

**Request**:
```bash
curl -X POST "http://localhost:5000/api/customers" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Jane Smith",
    "email": "jane@example.com",
    "phone": "+1-555-0002",
    "address": "456 Oak Ave, Springfield, IL 62702"
  }'
```

**Success Response** (201 Created):
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "name": "Jane Smith",
  "email": "jane@example.com",
  "phone": "+1-555-0002",
  "address": "456 Oak Ave, Springfield, IL 62702",
  "createdAt": "2024-01-15T14:30:00Z",
  "updatedAt": "2024-01-15T14:30:00Z"
}
```

**Validation Error Response** (400 Bad Request):
```json
{
  "status": 400,
  "title": "Validation Error",
  "detail": "One or more validation errors occurred.",
  "traceId": "0HNFQ8P7NJNB0:00000001",
  "errors": {
    "email": [
      "Email address is required",
      "Email must be a valid email address"
    ],
    "name": [
      "Name must be between 2 and 100 characters"
    ]
  },
  "timestamp": "2024-01-15T14:30:00Z"
}
```

**Conflict Error Response** (409 Conflict):
```json
{
  "status": 409,
  "title": "Conflict",
  "detail": "A customer with email 'jane@example.com' already exists",
  "traceId": "0HNFQ8P7NJNB0:00000002",
  "timestamp": "2024-01-15T14:30:00Z"
}
```

**Domain Event**:
When a customer is created:
- `CustomerCreatedDomainEvent` is published with:
  - CustomerId
  - Name
  - Email
  - Phone
  - Address
  - Timestamp
  - EventId (for tracking)
- Event handlers process the event for:
  - Audit logging (sent to Audit DB)
  - Message bus publishing (for async processes)

---

#### Update Customer
**Endpoint**: `PUT /api/customers/{id}`
**Authentication**: Required (Bearer Token)
**Description**: Update an existing customer

**Validation Rules**: Same as Create (name, email, phone, address)

**Request**:
```bash
curl -X PUT "http://localhost:5000/api/customers/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Smith",
    "email": "john.smith@example.com",
    "phone": "+1-555-0003",
    "address": "789 Pine Rd, Springfield, IL 62703"
  }'
```

**Success Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "John Smith",
  "email": "john.smith@example.com",
  "phone": "+1-555-0003",
  "address": "789 Pine Rd, Springfield, IL 62703",
  "createdAt": "2024-01-10T10:00:00Z",
  "updatedAt": "2024-01-15T14:45:00Z"
}
```

**Error Responses**:
- `400 Bad Request`: Validation error
- `401 Unauthorized`: Missing or invalid token
- `404 Not Found`: Customer not found
- `409 Conflict`: Email already used by another customer

**Domain Event**:
When a customer is updated:
- `CustomerUpdatedDomainEvent` is published with:
  - CustomerId
  - All updated customer data
  - Timestamp
  - EventId (for tracking)
- Event handlers process the event for:
  - Audit logging
  - Message bus publishing

---

#### Delete Customer
**Endpoint**: `DELETE /api/customers/{id}`
**Authentication**: Required (Bearer Token)
**Description**: Delete a customer

**Request**:
```bash
curl -X DELETE "http://localhost:5000/api/customers/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Success Response** (204 No Content):
```
(Empty body)
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid token
- `404 Not Found`: Customer not found

**Domain Event**:
When a customer is deleted:
- `CustomerDeletedDomainEvent` is published with:
  - CustomerId
  - Complete customer snapshot (name, email, phone, address at time of deletion)
  - Timestamp
  - EventId (for tracking)
- Event handlers process the event for:
  - Audit logging (records deletion with full snapshot)
  - Message bus publishing (for async cleanup processes)

---

## Error Handling

### Error Response Format

All error responses follow a consistent format:

```json
{
  "status": 400,
  "title": "Error Title",
  "detail": "Detailed error message",
  "traceId": "0HNFQ8P7NJNB0:00000001",
  "errors": {
    "fieldName": ["Error message 1", "Error message 2"]
  },
  "timestamp": "2024-01-15T14:30:00Z"
}
```

### HTTP Status Codes

| Status | Meaning | Common Cause |
|--------|---------|--------------|
| 200 | OK | Successful GET, PUT request |
| 201 | Created | Successful POST request |
| 204 | No Content | Successful DELETE request |
| 400 | Bad Request | Validation error, malformed request |
| 401 | Unauthorized | Missing or invalid authentication token |
| 404 | Not Found | Resource does not exist |
| 409 | Conflict | Duplicate email, business rule violation |
| 500 | Internal Server Error | Server error (rare with proper error handling) |

### Validation Errors

When validation fails (400 response), the `errors` object contains field-specific messages:

```json
{
  "status": 400,
  "title": "Validation Error",
  "detail": "One or more validation errors occurred.",
  "errors": {
    "name": [
      "Name is required",
      "Name must be between 2 and 100 characters"
    ],
    "email": [
      "Email must be a valid email address"
    ]
  },
  "timestamp": "2024-01-15T14:30:00Z"
}
```

---

## Examples

### Complete Login Flow with cURL

#### 1. Register a new user:
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alice@example.com",
    "password": "SecurePassword123!",
    "firstName": "Alice",
    "lastName": "Johnson"
  }'

# Response:
# {
#   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "tokenType": "Bearer",
#   "expiresIn": 3600,
#   "expiresAt": "2024-01-15T15:30:00Z"
# }
```

#### 2. Save the token:
```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

#### 3. Create a customer:
```bash
curl -X POST "http://localhost:5000/api/customers" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Bob Wilson",
    "email": "bob@example.com",
    "phone": "+1-555-0100",
    "address": "123 Business Ave, Corporate City, CA 90001"
  }'
```

#### 4. Get all customers:
```bash
curl -X GET "http://localhost:5000/api/customers" \
  -H "Authorization: Bearer $TOKEN"
```

#### 5. Get specific customer:
```bash
curl -X GET "http://localhost:5000/api/customers/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer $TOKEN"
```

#### 6. Update customer:
```bash
curl -X PUT "http://localhost:5000/api/customers/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Bob Wilson Updated",
    "email": "bob.wilson@example.com",
    "phone": "+1-555-0101",
    "address": "456 Business Ave, Corporate City, CA 90002"
  }'
```

#### 7. Delete customer:
```bash
curl -X DELETE "http://localhost:5000/api/customers/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer $TOKEN"
```

### Using Swagger UI

1. Navigate to `http://localhost:5000/api-docs`
2. Click "Authorize" button
3. Enter `Bearer {your_token}` in the authorization dialog
4. Click "Authorize"
5. Now you can test all endpoints interactively
6. Click "Try it out" on any endpoint
7. Fill in parameters and click "Execute"
8. View the response in real-time

---

## Rate Limiting

### Limits
- **Per-User**: 100 requests per minute
- **Global**: 5000 requests per hour

### Response Headers
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 2024-01-15T14:31:00Z
```

### 429 Too Many Requests
When rate limit is exceeded:

```json
{
  "status": 429,
  "title": "Too Many Requests",
  "detail": "Rate limit exceeded. Try again after 2024-01-15T14:31:00Z",
  "retryAfter": 60,
  "timestamp": "2024-01-15T14:30:00Z"
}
```

---

## Testing

### Integration Testing

Using HttpClient in C#:

```csharp
var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

// Login
var loginRequest = new { email = "test@example.com", password = "Password123!" };
var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
var loginResult = await loginResponse.Content.ReadAsAsync<LoginResponse>();
var token = loginResult.Token;

// Add token to headers
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);

// Create customer
var createRequest = new { 
    name = "Test User", 
    email = "test@example.com", 
    phone = "+1-555-0000", 
    address = "123 Test St" 
};
var createResponse = await client.PostAsJsonAsync("/api/customers", createRequest);
var customer = await createResponse.Content.ReadAsAsync<CustomerResponse>();

// Get customers
var getResponse = await client.GetAsync("/api/customers");
var customers = await getResponse.Content.ReadAsAsync<List<CustomerResponse>>();

// Update customer
var updateRequest = new { 
    name = "Updated User", 
    email = "updated@example.com", 
    phone = "+1-555-0001", 
    address = "456 Updated Ave" 
};
var updateResponse = await client.PutAsJsonAsync(
    $"/api/customers/{customer.Id}", 
    updateRequest
);

// Delete customer
var deleteResponse = await client.DeleteAsync($"/api/customers/{customer.Id}");
```

### API Documentation for Developers

The complete API specification is available in OpenAPI 3.0 format at:
- **JSON**: `http://localhost:5000/api-docs/v1/swagger.json`
- **YAML**: Can be converted from JSON for other tools

### Environment Variables

Required environment variables for API:

```bash
# JWT Configuration
JWT_SECRET_KEY=your-super-secret-key-min-32-chars-long
JWT_ISSUER=CleanArchitecture
JWT_AUDIENCE=CleanArchitectureAPI
JWT_EXPIRATION_HOURS=1

# Database
POSTGRES_CONNECTION_STRING=Server=localhost;Port=5432;Database=cleanarch;User Id=postgres;Password=password

# Logging
SERILOG_MIN_LEVEL=Information
```

---

## Support

For API support:
- **Email**: support@cleanarchitecture.local
- **Documentation**: https://github.com/yourusername/CleanArchitecture_dotnet10
- **Issues**: GitHub Issues page

---

## License

This API is licensed under the MIT License. See [LICENSE.txt](../LICENSE.txt) for details.

---

**Last Updated**: January 15, 2024  
**API Version**: v1.0  
**Status**: Production Ready
