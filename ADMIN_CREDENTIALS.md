# 🔐 ADMIN CREDENTIALS - SAV Manager API

## Default Admin Account (Auto-seeded at Startup)

```
📧 Email:    admin@savmanager.com
🔑 Password: Admin@123456
👤 Name:     System Administrator
```

---

## How to Use

### 1. **Login with Admin Account**

```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
	"email": "admin@savmanager.com",
	"password": "Admin@123456"
  }'
```

**Response:**
```json
{
  "accessToken": "eyJ0eXAiOiJKV1QiLCJhbGc...",
  "refreshToken": "eyJ0eXAiOiJKV1QiLCJhbGc...",
  "userId": 1,
  "fullName": "System Administrator",
  "role": "Admin"
}
```

---

### 2. **Create Additional Admin Accounts (via API)**

```bash
curl -X POST "https://localhost:5001/api/auth/create-admin" \
  -H "Content-Type: application/json" \
  -d '{
	"email": "newadmin@example.com",
	"password": "NewAdmin@123",
	"fullName": "New Administrator"
  }'
```

**Response:**
```json
{
  "message": "Admin user created successfully.",
  "email": "newadmin@example.com"
}
```

---

### 3. **Test in Swagger UI**

1. Navigate to: `https://localhost:5001/swagger`
2. Click on **POST /api/auth/login**
3. Enter credentials:
   ```json
   {
	 "email": "admin@savmanager.com",
	 "password": "Admin@123456"
   }
   ```
4. Execute and copy the `accessToken`
5. Click **Authorize** button (top right)
6. Paste: `Bearer {your_accessToken}`
7. Now all protected endpoints are accessible!

---

### 4. **Access Protected Admin Endpoints**

With the Bearer token, you can access admin-only endpoints:

```bash
curl -X GET "https://localhost:5001/api/admin/dashboard" \
  -H "Authorization: Bearer {accessToken}"
```

---

## Important Notes

⚠️ **Security Considerations:**

1. **Change Default Password Immediately** in production
   - Update in database or via API after first login

2. **Restrict `/api/auth/create-admin` endpoint**
   - In `SAV.API/Controllers/AuthController.cs`, change:
   ```csharp
   [AllowAnonymous]  // ❌ REMOVE THIS
   ```
   To:
   ```csharp
   [Authorize(Roles = "Admin")]  // ✅ ADD THIS
   ```

3. **Environment Variables**
   - Store JWT keys securely in `appsettings.json` or Azure Key Vault
   - Never hardcode credentials

4. **HTTPS Only**
   - Always use HTTPS in production
   - Never transmit tokens over HTTP

---

## Database Seeding

The admin account is **automatically created** when the API starts up:

- Location: `SAV.API/Program.cs` (lines 160-182)
- Happens once per application startup
- If account exists, it's skipped gracefully
- Check logs for confirmation: `✅ Default admin account created successfully.`

---

## Troubleshooting

### Issue: Admin account not created
**Solution:**
1. Check API logs for errors
2. Verify database connection string in `appsettings.json`
3. Ensure database migrations are applied: `Update-Database`
4. Manually create via endpoint:
   ```bash
   POST /api/auth/create-admin
   ```

### Issue: Login fails with "Invalid credentials"
**Solution:**
1. Verify email and password are correct (case-sensitive)
2. Ensure user is marked as `IsActive = true` in database
3. Check that "Admin" role exists in database

### Issue: Token expired
**Solution:**
1. Use the `refreshToken` from login response:
   ```bash
   POST /api/auth/refresh-token
   ```
2. Or login again to get new tokens

---

## Test Workflow

```bash
# 1. Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@savmanager.com","password":"Admin@123456"}'

# Copy the accessToken

# 2. Use token for protected API calls
curl -X GET https://localhost:5001/api/admin/users \
  -H "Authorization: Bearer {accessToken}"

# 3. Refresh expired token
curl -X POST https://localhost:5001/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"{refreshToken_from_step_1}"}'

# 4. Logout
curl -X POST https://localhost:5001/api/auth/logout
```

---

## Files Modified

| File | Change |
|------|--------|
| `SAV.Infrastructure/Identity/AuthenticationService.cs` | Added `CreateAdminAsync()` method |
| `SAV.Shared/Interfaces/IAuthenticationService.cs` | Added interface method signature |
| `SAV.API/Controllers/AuthController.cs` | Added `/api/auth/create-admin` endpoint |
| `SAV.API/Program.cs` | Added admin seeding on startup |
| `SAV.Shared/DTOs/Auth/CreateAdminRequest.cs` | Created DTO class |

---

**Created:** 2026-01-15  
**Status:** ✅ Ready for Testing  
**Security Level:** Development (⚠️ Update before production deployment)
