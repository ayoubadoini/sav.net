# API Documentation - Endpoints Reference

## Authentication Endpoints

### Register
```
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "fullName": "John Doe"
}

Response: 200 OK
{
  "success": true
}
```

### Login
```
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}

Response: 200 OK
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "Base64EncodedToken",
  "userId": 1,
  "fullName": "John Doe",
  "role": "Client"
}
```

### Refresh Token
```
POST /api/auth/refresh-token
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "refreshToken": "Base64EncodedToken"
}

Response: 200 OK
{
  "accessToken": "newToken",
  "refreshToken": "newRefreshToken",
  ...
}
```

### Logout
```
POST /api/auth/logout
Authorization: Bearer {accessToken}

Response: 200 OK
```

## Complaints Endpoints

### Get All Complaints
```
GET /api/complaints
Authorization: Bearer {accessToken}

Response: 200 OK
[
  {
	"id": 1,
	"title": "Product not working",
	"description": "Device stopped working after 2 days",
	"status": "Pending",
	"createdAt": "2026-01-17T10:30:00Z",
	"clientId": 1,
	"articleId": 5,
	"articleName": "Central Heating System"
  }
]
```

### Get Complaint by ID
```
GET /api/complaints/{id}
Authorization: Bearer {accessToken}

Response: 200 OK
{
  "id": 1,
  ...
}
```

### Create Complaint
```
POST /api/complaints
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "title": "Issue with product",
  "description": "Detailed description",
  "articleId": 5,
  "clientId": 1
}

Response: 201 Created
Location: /api/complaints/2
{
  "id": 2,
  "title": "Issue with product",
  ...
}
```

### Update Complaint
```
PUT /api/complaints/{id}
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "title": "Updated title",
  "description": "Updated description",
  "status": "InProgress"
}

Response: 204 No Content
```

### Delete Complaint
```
DELETE /api/complaints/{id}
Authorization: Bearer {accessToken}

Response: 204 No Content
```

## Interventions Endpoints

### Get All Interventions
```
GET /api/interventions
Authorization: Bearer {accessToken}

Response: 200 OK
[
  {
	"id": 1,
	"date": "2026-01-20T09:00:00Z",
	"description": "Maintenance and repair",
	"laborCost": 150.00,
	"status": "Planned",
	"complaintId": 1,
	"technicianId": 1,
	"technicianName": "John Smith"
  }
]
```

### Get Intervention by ID
```
GET /api/interventions/{id}
Authorization: Bearer {accessToken}

Response: 200 OK
{ ... }
```

### Schedule Intervention
```
POST /api/interventions
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "date": "2026-01-20T09:00:00Z",
  "description": "Repair and maintenance",
  "laborCost": 150.00,
  "complaintId": 1,
  "technicianId": 1
}

Response: 201 Created
{ ... }
```

### Update Intervention
```
PUT /api/interventions/{id}
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "status": "Completed",
  "laborCost": 200.00
}

Response: 204 No Content
```

## Clients Endpoints

### Get All Clients
```
GET /api/clients
Authorization: Bearer {accessToken}

Response: 200 OK
[
  {
	"id": 1,
	"fullName": "John Doe",
	"email": "john@example.com",
	"phone": "+33123456789",
	"address": "123 Main St, Paris"
  }
]
```

### Get Client by ID
```
GET /api/clients/{id}
Authorization: Bearer {accessToken}

Response: 200 OK
{ ... }
```

### Create Client
```
POST /api/clients
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "fullName": "Jane Smith",
  "email": "jane@example.com",
  "phone": "+33987654321",
  "address": "456 Oak Ave, Lyon"
}

Response: 201 Created
{ ... }
```

### Update Client
```
PUT /api/clients/{id}
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "phone": "+33111111111",
  "address": "New Address"
}

Response: 204 No Content
```

## Invoices Endpoints

### Get Invoices
```
GET /api/invoices
Authorization: Bearer {accessToken}

Response: 200 OK
[
  {
	"id": 1,
	"invoiceNumber": "INV-001",
	"issueDate": "2026-01-17T10:00:00Z",
	"subTotal": 150.00,
	"taxAmount": 30.00,
	"totalAmount": 180.00,
	"isPaid": false,
	"interventionId": 1
  }
]
```

### Generate Invoice PDF
```
GET /api/invoices/{id}/pdf
Authorization: Bearer {accessToken}

Response: 200 OK
[PDF Content]
```

## Articles Endpoints

### Get Articles
```
GET /api/articles
Authorization: Bearer {accessToken}

Response: 200 OK
[
  {
	"id": 1,
	"name": "Central Heating System",
	"serialNumber": "CHR-2023-001",
	"purchaseDate": "2023-06-01",
	"warrantyEndDate": "2025-06-01",
	"isUnderWarranty": true,
	"categoryId": 1,
	"clientId": 1
  }
]
```

## Error Responses

### 400 Bad Request
```json
{
  "type": "ValidationException",
  "title": "One or more validation errors occurred",
  "status": 400,
  "errors": {
	"email": ["Email is required"]
  }
}
```

### 401 Unauthorized
```json
{
  "type": "UnauthorizedException",
  "title": "Unauthorized",
  "status": 401,
  "message": "Invalid credentials"
}
```

### 403 Forbidden
```json
{
  "type": "ForbiddenException",
  "title": "Forbidden",
  "status": 403,
  "message": "You don't have permission"
}
```

### 404 Not Found
```json
{
  "type": "NotFoundException",
  "title": "Not Found",
  "status": 404,
  "message": "Resource not found"
}
```

### 500 Internal Server Error
```json
{
  "type": "ServerException",
  "title": "Internal Server Error",
  "status": 500,
  "message": "An unexpected error occurred"
}
```

## Status Codes

| Code | Description |
|------|-------------|
| 200 | OK |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 422 | Unprocessable Entity |
| 500 | Internal Server Error |

## Headers

All requests should include:
```
Content-Type: application/json
Authorization: Bearer {accessToken}
```

## Rate Limiting

- No rate limiting implemented (can be added)
- Request timeout: 30 seconds

## Pagination

Future endpoints will support:
```
GET /api/complaints?page=1&pageSize=10&sortBy=createdAt&sortDirection=desc
```

---

For more details, visit Swagger UI: `https://localhost:7001/swagger`
