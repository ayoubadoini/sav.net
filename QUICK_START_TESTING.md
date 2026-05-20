# ⚡ Quick Start Guide - Testing Your Enterprise Backend

## 🚀 Start the API

```bash
cd C:\Users\ayoub\Desktop\S2\.NET\SAVSolution\SAV.API
dotnet run
```

**Access Swagger:** https://localhost:5001/swagger

---

## 🔐 Get a JWT Token

### Option 1: Via Swagger UI

1. Open Swagger: https://localhost:5001/swagger
2. Find **Auth** section
3. Click **Register** or **Login** endpoint
4. Fill credentials:
   ```json
   {
	 "email": "admin@example.com",
	 "password": "Admin@123"
   }
   ```
5. Copy the returned `token` value

### Option 2: Via cURL

```powershell
$body = @{
	email = "admin@example.com"
	password = "Admin@123"
} | ConvertTo-Json

$response = Invoke-WebRequest `
	-Uri "https://localhost:5001/api/auth/login" `
	-Method POST `
	-ContentType "application/json" `
	-Body $body `
	-SkipCertificateCheck

$token = ($response.Content | ConvertFrom-Json).token
Write-Host "Token: $token"
```

---

## 🧪 Test Authorization Policies

### 1. Test AdminOnly Policy

```bash
# ✅ SHOULD WORK - Admin deleting complaint
DELETE https://localhost:5001/api/complaints/1
Authorization: Bearer {admin_token}

# Response: 204 No Content
```

```bash
# ❌ SHOULD FAIL - Client deleting complaint
DELETE https://localhost:5001/api/complaints/1
Authorization: Bearer {client_token}

# Response: 403 Forbidden
```

### 2. Test ClientOnly Policy

```bash
# ✅ SHOULD WORK - Client creating complaint
POST https://localhost:5001/api/complaints
Authorization: Bearer {client_token}
Content-Type: application/json

{
  "title": "Product not working",
  "description": "Cannot start device",
  "clientId": 42,
  "articleId": 1
}

# Response: 201 Created
```

```bash
# ❌ SHOULD FAIL - Technician creating complaint
POST https://localhost:5001/api/complaints
Authorization: Bearer {technician_token}

# Response: 403 Forbidden
```

### 3. Test AdminOrTechnician Policy

```bash
# ✅ SHOULD WORK - Admin updating intervention
PUT https://localhost:5001/api/interventions/1
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "status": "Completed",
  "completedDate": "2026-01-15"
}

# Response: 200 OK
```

```bash
# ✅ SHOULD WORK - Technician updating intervention
PUT https://localhost:5001/api/interventions/1
Authorization: Bearer {technician_token}

# Response: 200 OK
```

```bash
# ❌ SHOULD FAIL - Client updating intervention
PUT https://localhost:5001/api/interventions/1
Authorization: Bearer {client_token}

# Response: 403 Forbidden
```

---

## 📊 Test Audit Fields

### 1. Create a Complaint

```bash
POST https://localhost:5001/api/complaints
Authorization: Bearer {client_token}
Content-Type: application/json

{
  "title": "Screen flickering",
  "description": "Monitor keeps flickering",
  "clientId": 42,
  "articleId": 5
}
```

**Response:**
```json
{
  "id": 100,
  "title": "Screen flickering",
  "description": "Monitor keeps flickering",
  "status": "Pending",
  "clientId": 42,
  "articleId": 5,
  "createdBy": 42,
  "createdDate": "2026-01-15T14:30:00Z",
  "updatedBy": null,
  "updatedDate": null,
  "isDeleted": false
}
```

✅ **Notice:**
- `createdBy` = 42 (auto-populated)
- `createdDate` = 2026-01-15T14:30:00Z (auto-populated)
- UTC timezone ✅

### 2. Update the Complaint

```bash
PUT https://localhost:5001/api/complaints/100
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "title": "Screen flickering - URGENT",
  "description": "Monitor keeps flickering - needs immediate attention"
}
```

**Response:**
```json
{
  "id": 100,
  "title": "Screen flickering - URGENT",
  ...
  "createdBy": 42,
  "createdDate": "2026-01-15T14:30:00Z",
  "updatedBy": 1,
  "updatedDate": "2026-01-15T14:45:00Z",
  "isDeleted": false
}
```

✅ **Notice:**
- `createdBy` = 42 (unchanged - protected) ✅
- `createdDate` = 2026-01-15T14:30:00Z (unchanged - protected) ✅
- `updatedBy` = 1 (admin's user ID - auto-populated) ✅
- `updatedDate` = 2026-01-15T14:45:00Z (auto-populated) ✅

### 3. Delete the Complaint (Soft Delete)

```bash
DELETE https://localhost:5001/api/complaints/100
Authorization: Bearer {admin_token}
```

**Response:** 204 No Content

### 4. Verify Soft Delete

```bash
# Get all complaints
GET https://localhost:5001/api/complaints
Authorization: Bearer {admin_token}
```

✅ **Notice:**
- Complaint 100 is NOT in the list (auto-excluded by query filter)
- Only active complaints returned

### 5. Query Database Directly

```sql
-- SQL Server Management Studio or VS Code with SQL Server extension

-- Show ALL complaints including deleted
SELECT Id, Title, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, IsDeleted, DeletedDate
FROM Complaints
WHERE Id = 100;

-- Result:
-- Id: 100
-- Title: Screen flickering - URGENT
-- CreatedBy: 42
-- CreatedDate: 2026-01-15 14:30:00.0000000
-- UpdatedBy: 1
-- UpdatedDate: 2026-01-15 14:45:00.0000000
-- IsDeleted: 1 (true - soft deleted)
-- DeletedDate: 2026-01-15 14:50:00.0000000
```

---

## 🔍 Test Current User Service

### Get Current User Info

```csharp
// In any controller
public class TestController : ControllerBase
{
	private readonly ICurrentUserService _currentUserService;

	public TestController(ICurrentUserService currentUserService)
	{
		_currentUserService = currentUserService;
	}

	[HttpGet("whoami")]
	[Authorize]
	public IActionResult WhoAmI()
	{
		var userId = _currentUserService.GetCurrentUserId();
		var email = _currentUserService.GetCurrentUserEmail();
		var role = _currentUserService.GetCurrentUserRole();
		var fullName = _currentUserService.GetCurrentUserFullName();

		return Ok(new
		{
			userId,
			email,
			role,
			fullName,
			isAdmin = _currentUserService.IsAdmin(),
			isTechnician = _currentUserService.IsTechnician(),
			isClient = _currentUserService.IsClient()
		});
	}
}
```

**Request:**
```bash
GET https://localhost:5001/api/test/whoami
Authorization: Bearer {admin_token}
```

**Response:**
```json
{
  "userId": 1,
  "email": "admin@example.com",
  "role": "Admin",
  "fullName": "Admin User",
  "isAdmin": true,
  "isTechnician": false,
  "isClient": false
}
```

---

## 📋 Test Checklist

### Authorization ✅
- [ ] Admin can delete complaints
- [ ] Client cannot delete complaints
- [ ] Client can create complaints
- [ ] Technician cannot create complaints
- [ ] Admin/Technician can update interventions
- [ ] Client cannot update interventions
- [ ] Ownership checks work (client can only access own records)

### Audit Fields ✅
- [ ] CreatedBy populated on create
- [ ] CreatedDate set to UTC on create
- [ ] UpdatedBy populated on update
- [ ] UpdatedDate set to UTC on update
- [ ] CreatedBy cannot be changed (protected)
- [ ] CreatedDate cannot be changed (protected)
- [ ] IsDeleted = true on soft delete
- [ ] DeletedDate set to UTC on soft delete
- [ ] Deleted records excluded from queries

### Soft Delete ✅
- [ ] GetAll() doesn't return deleted records
- [ ] Query filter applies automatically
- [ ] IgnoreQueryFilters() can bypass filter
- [ ] Indexes on IsDeleted improve performance
- [ ] Deleted records preserved in database

---

## 🐛 Troubleshooting

### 401 Unauthorized
**Problem:** Token invalid or expired
**Solution:**
- Get a new token
- Check token format: `Authorization: Bearer {token}`
- Verify token hasn't expired

### 403 Forbidden
**Problem:** User doesn't have required role
**Solution:**
- Check role in JWT token
- Use token from different user role
- Verify policy name is correct

### 400 Bad Request
**Problem:** Invalid request body
**Solution:**
- Check JSON format
- Verify required fields are present
- Check data types match

### 500 Internal Server Error
**Problem:** Server error
**Solution:**
- Check application logs in console
- Verify database connection
- Check stored procedure/migration status

---

## 📚 Next Steps

1. **Implement remaining handlers:**
   - Create MediatR commands/queries for Interventions
   - Create MediatR commands/queries for Invoices
   - Create MediatR commands/queries for Clients/Technicians

2. **Connect WebUI (Razor Pages):**
   - Add HttpClient to call API
   - Store JWT token in session
   - Pass token in Authorization header
   - Add login/logout pages

3. **Add more authorization policies:**
   - Resource-based (check ownership)
   - Claim-based (custom claims)
   - Dynamic policies

4. **Enhance audit system:**
   - Add audit log table
   - Track all changes (not just soft delete)
   - Add change history UI

---

## 💡 Tips

- Always use HTTPS in production (JWT tokens in Authorization header)
- Rotate JWT secrets regularly
- Use strong passwords for test users
- Monitor audit logs for suspicious activity
- Implement rate limiting to prevent brute force
- Keep tokens short-lived (15 mins) with refresh tokens (7 days)

---

**Happy Testing! 🚀**

If you encounter issues, check the application console output for detailed error messages.
