# Quick Reference - API Documentation

## 🚀 Getting Started

### Start the API
```bash
cd src/Web.Api
dotnet run
```

### Access Swagger UI
```
http://localhost:5000/api-docs
```

---

## 📚 Documentation Files

| File | Purpose |
|------|---------|
| [API_USAGE_GUIDE.md](./API_USAGE_GUIDE.md) | **Start here** - Complete guide with examples |
| [SWAGGER_SETUP.md](./SWAGGER_SETUP.md) | Technical setup and configuration |
| [API_DOCUMENTATION_SUMMARY.md](./API_DOCUMENTATION_SUMMARY.md) | Implementation summary |
| [CUSTOMER_CRUD_REFACTORING.md](./CUSTOMER_CRUD_REFACTORING.md) | CRUD implementation details |

---

## 🔑 Quick Authentication

### 1. Register
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123!","firstName":"John","lastName":"Doe"}'
```

### 2. Login
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123!"}'
```

### 3. Use Token
```bash
curl -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  http://localhost:5000/api/customers
```

---

## 📡 Endpoints

### Auth (No Authentication)
- `POST /api/auth/register` - Create account
- `POST /api/auth/login` - Get token

### Customers (Requires Token)
- `GET /api/customers` - List all
- `GET /api/customers/{id}` - Get one
- `POST /api/customers` - Create
- `PUT /api/customers/{id}` - Update
- `DELETE /api/customers/{id}` - Delete

### Health (No Authentication)
- `GET /health` - Status check

---

## 🔐 Token Usage

Token format:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

In Swagger UI:
1. Click "Authorize" button
2. Enter: `Bearer {token}`
3. Click "Authorize"

---

## ✅ Validation Rules

| Field | Rules |
|-------|-------|
| name | Required, 2-100 chars |
| email | Required, unique, valid format |
| phone | Optional, valid format |
| address | Optional, max 500 chars |

---

## 📊 Response Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success (GET, PUT) |
| 201 | Created (POST) |
| 204 | No Content (DELETE) |
| 400 | Bad Request (validation) |
| 401 | Unauthorized (no token) |
| 404 | Not Found |
| 409 | Conflict (duplicate) |

---

## 🔍 Error Response

```json
{
  "status": 400,
  "title": "Validation Error",
  "detail": "One or more validation errors occurred.",
  "errors": {
    "email": ["Email is required"]
  },
  "timestamp": "2024-01-15T14:30:00Z"
}
```

---

## 💡 Pro Tips

✨ **Swagger UI Features**:
- Click "Try It Out" to test endpoints
- View request/response examples
- See real-time response times
- Filter endpoints by tag

🎯 **Best Practices**:
- Save your JWT token for repeated requests
- Check expiration time in response
- Use correct Content-Type headers
- Include all required fields

🔒 **Security**:
- Keep tokens secure
- Use HTTPS in production
- Tokens expire after 1 hour
- Regenerate tokens as needed

---

## 📞 Need Help?

1. **Read**: [API_USAGE_GUIDE.md](./API_USAGE_GUIDE.md)
2. **Check**: Swagger UI error messages
3. **Debug**: Look at server logs
4. **Contact**: support@cleanarchitecture.local

---

## 🎓 Learning Path

1. **Start**: Read API_USAGE_GUIDE.md (15 min)
2. **Explore**: Test in Swagger UI (10 min)
3. **Practice**: Try cURL examples (15 min)
4. **Integrate**: Use in your app (varies)

---

**Version**: 1.0  
**Status**: ✅ Ready to Use  
**Last Updated**: January 15, 2024
