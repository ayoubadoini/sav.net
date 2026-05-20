# 🚀 QUICK START - Admin Account

## Your Admin Credentials

```
📧 Email:    admin@savmanager.com
🔑 Password: Admin@123456
```

---

## Login Immediately

### Option 1: Via Swagger UI (Easiest)
1. Run your API: `dotnet run`
2. Go to: `https://localhost:5001/swagger`
3. Click **POST /api/auth/login**
4. Paste this:
```json
{
  "email": "admin@savmanager.com",
  "password": "Admin@123456"
}
```
5. Click **Execute**
6. Copy the `accessToken` from response
7. Click **Authorize** (top right) and paste: `Bearer {accessToken}`

---

### Option 2: Via Postman
1. Create new request: `POST https://localhost:5001/api/auth/login`
2. Body (JSON):
```json
{
  "email": "admin@savmanager.com",
  "password": "Admin@123456"
}
```
3. Send
4. Copy `accessToken` 
5. Add to headers of future requests: `Authorization: Bearer {token}`

---

### Option 3: Via cURL
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@savmanager.com","password":"Admin@123456"}'
```

---

## ✅ What Just Happened

- ✅ Admin account created automatically at app startup
- ✅ Admin user has "Admin" role assigned
- ✅ JWT tokens enabled for all endpoints
- ✅ Refresh token system active
- ✅ Admin can access protected endpoints

---

## Next Steps

1. **Login and get token** (see above)
2. **Use token** in Authorization header for protected APIs
3. **Create more admins** via: `POST /api/auth/create-admin`
4. **Check logs** for seeding confirmation: `✅ Default admin account created successfully.`

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Database not created | Run: `Update-Database` in Package Manager Console |
| Admin not found | Ensure API ran once to seed data |
| Login fails | Check email/password spelling (case-sensitive) |
| 401 Unauthorized | Make sure to include `Authorization: Bearer {token}` header |

---

**Status:** ✅ Ready to use!
