# SAV Management System - Testing Guide

## 🎯 Current Status

✅ **Backend**: Running on `http://localhost:5050`
✅ **Frontend**: Running on `http://localhost:3000`
✅ **Database**: SQL Server with initial schema
✅ **Authentication**: JWT infrastructure ready

---

## 📝 Test Cases

### 1. Registration Test

**Endpoint**: `POST http://localhost:5050/api/auth/register`

**Using Swagger** (Recommended):
1. Navigate to `http://localhost:5050/swagger/index.html`
2. Expand the "Auth" section
3. Click "Try it out" on the `/api/auth/register` endpoint
4. Enter sample data:
   ```json
   {
	 "email": "client@example.com",
	 "password": "Password123!",
	 "fullName": "John Doe"
   }
   ```
5. Click "Execute"

**Using cURL**:
```bash
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
	"email": "client@example.com",
	"password": "Password123!",
	"fullName": "John Doe"
  }'
```

**Expected Response**:
```json
{
  "statusCode": 200,
  "message": "Registration successful",
  "data": true
}
```

---

### 2. Login Test

**Endpoint**: `POST http://localhost:5050/api/auth/login`

**Using Swagger**:
1. Navigate to `http://localhost:5050/swagger/index.html`
2. Expand the "Auth" section
3. Click "Try it out" on the `/api/auth/login` endpoint
4. Enter credentials:
   ```json
   {
	 "email": "client@example.com",
	 "password": "Password123!"
   }
   ```
5. Click "Execute"

**Using cURL**:
```bash
curl -X POST http://localhost:5050/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
	"email": "client@example.com",
	"password": "Password123!"
  }'
```

**Expected Response**:
```json
{
  "statusCode": 200,
  "message": "Login successful",
  "data": {
	"accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
	"refreshToken": "base64-encoded-refresh-token",
	"userId": 1,
	"fullName": "John Doe",
	"role": "Client"
  }
}
```

---

### 3. React Frontend - Login Page

1. Navigate to `http://localhost:3000`
2. You should see the **Login Page**
3. Enter credentials:
   - Email: `client@example.com`
   - Password: `Password123!`
4. Click "Login"
5. Token is automatically stored in `localStorage`
6. Redirect to Dashboard (currently placeholder)

---

### 4. Check Token in Browser

1. Open DevTools (F12)
2. Go to **Application** tab
3. Navigate to **Storage → Local Storage → http://localhost:3000**
4. You should see:
   - `authToken` - Access token
   - `refreshToken` - Refresh token (if implemented)

---

### 5. Network Request Test

1. Open DevTools (F12)
2. Go to **Network** tab
3. Attempt a protected API call from the React app
4. Verify request headers include:
   ```
   Authorization: Bearer <your-access-token>
   ```

---

## 🔧 Create Test Data Manually

### Via SQL Server (SSMS or Azure Data Studio)

```sql
-- Check created users
SELECT * FROM AspNetUsers;

-- Check roles
SELECT * FROM AspNetRoles;

-- View clients
SELECT * FROM Clients;

-- View complaints
SELECT * FROM Complaints;
```

### Via API Endpoints (Using Swagger)

**Create Admin User** (Optional - for later SAV manager testing):
```json
{
  "email": "admin@example.com",
  "password": "AdminPassword123!",
  "fullName": "Admin User"
}
```

---

## 📊 Swagger UI Features

Access the full API documentation at: **http://localhost:5050/swagger/index.html**

### Available Features:
- ✅ Authentication endpoints
- ✅ Auto-generated request/response models
- ✅ "Try it out" testing directly in browser
- ✅ JWT bearer token input field
- ✅ Response status codes and descriptions

### To Use JWT in Swagger:
1. Login and copy the `accessToken`
2. Scroll to top of Swagger page
3. Click "Authorize" button
4. Paste token in "Bearer" field
5. Click "Authorize"
6. Now all requests include the token automatically

---

## 🧪 Frontend Component Testing

### Login Flow
```
http://localhost:3000
  ↓
[Login Form fills data]
  ↓
[Submit → POST /api/auth/login]
  ↓
[Token stored in localStorage]
  ↓
[Redirect to /dashboard]
```

### Protected Route
- Routes wrapped in `<PrivateRoute>` component
- Check authentication before rendering
- Redirect to login if not authenticated

### NavBar
- Shows when authenticated
- Displays user role (if implemented)
- Logout button clears tokens

---

## 🔍 Debugging Tips

### Backend
1. **Check API is running**:
   ```bash
   curl http://localhost:5050/health
   ```

2. **View API logs** in terminal where `dotnet run` was executed

3. **SQL Server check**:
   - Open SSMS
   - Connect to `(localdb)\mssqllocaldb`
   - View database: `SAVDb`
   - Check tables in schema

4. **JWT verification**:
   - Use https://jwt.io to decode tokens
   - Verify claims: `NameIdentifier`, `Email`, `Name`, `Role`

### Frontend
1. **Check browser console** (DevTools → Console)
   - Look for 404, 401, 500 errors
   - Check CORS warnings

2. **Check Network tab** (DevTools → Network)
   - Verify API requests go to `http://localhost:5050`
   - Check request/response headers
   - Look for failed requests

3. **React errors** (DevTools → Sources)
   - Set breakpoints in component code
   - Check state management

---

## 🚀 Next Test Steps

1. ✅ **Registration works** → Create admin and SAV manager accounts
2. ✅ **Login works** → Store token and redirect
3. ⏳ **Create Complaint endpoint** → POST to `/api/complaints`
4. ⏳ **List Complaints endpoint** → GET from `/api/complaints`
5. ⏳ **Dashboard page** → Display KPI cards with data
6. ⏳ **Complaints page** → Fetch and display complaints
7. ⏳ **SignalR notifications** → Real-time updates

---

## 📋 Checklist for Features

### Authentication (In Progress)
- ✅ Register endpoint
- ✅ Login endpoint
- ✅ JWT token generation
- ⏳ Refresh token endpoint
- ⏳ Logout endpoint
- ⏳ Password reset

### API Endpoints (To Do)
- ⏳ Complaints CRUD
- ⏳ Articles CRUD
- ⏳ Clients CRUD
- ⏳ Interventions CRUD
- ⏳ Invoices CRUD
- ⏳ Categories CRUD

### Frontend Components (To Do)
- ⏳ Dashboard with KPIs
- ⏳ Complaints management page
- ⏳ Create complaint form
- ⏳ Client management (Admin/SAV only)
- ⏳ File upload for complaints
- ⏳ PDF export for invoices

### Features (To Do)
- ⏳ File upload service
- ⏳ PDF generation
- ⏳ SignalR real-time notifications
- ⏳ Email notifications
- ⏳ Search & filtering
- ⏳ Pagination
- ⏳ Reporting

---

## 🛠️ Troubleshooting

### "Cannot reach localhost:5050"
- **Solution**: Check backend is running: `dotnet run` in SAV.API folder
- **Check**: `http://localhost:5050/swagger` should load

### "Login returns 401 Unauthorized"
- **Cause**: Wrong password or user doesn't exist
- **Solution**: Register a new user first
- **Verify**: Email/password in database matches request

### "Frontend shows 404 Not Found"
- **Cause**: API endpoint not implemented yet
- **Check**: Swagger UI at `/swagger` for available endpoints
- **Expected**: Only `/api/auth/login` and `/api/auth/register` work currently

### "Token not being sent in requests"
- **Cause**: JWT interceptor not configured properly
- **Check**: `src/services/apiClient.ts`
- **Verify**: Token exists in localStorage

### "CORS errors in browser console"
- **Cause**: Frontend URL not in CORS policy
- **Solution**: Backend allows `*` (all origins) in CORS policy
- **Check**: `Program.cs` CORS configuration

---

## 📞 Quick Reference

| Component | URL | Status |
|-----------|-----|--------|
| Backend API | `http://localhost:5050` | ✅ Running |
| Swagger UI | `http://localhost:5050/swagger` | ✅ Available |
| Frontend App | `http://localhost:3000` | ✅ Running |
| Database | `(localdb)\mssqllocaldb` | ✅ Ready |

| Endpoint | Method | Status |
|----------|--------|--------|
| `/api/auth/register` | POST | ✅ Working |
| `/api/auth/login` | POST | ✅ Working |
| `/api/auth/logout` | POST | ⏳ Placeholder |
| Other endpoints | * | ⏳ To be implemented |

---

## 📝 Sample Test Data

```
Email: testuser@example.com
Password: TestPassword123!
Full Name: Test User

Email: admin@example.com
Password: AdminPassword123!
Full Name: Admin User

Email: savmanager@example.com
Password: ManagerPassword123!
Full Name: SAV Manager
```

---

**Ready to test? Start with the Registration Test above! 🚀**
