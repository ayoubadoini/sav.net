# 🛠️ Quick Reference Guide - SAV Management System

## 🚀 Quick Start Commands

### Start Everything

**Terminal 1 - Backend API**
```bash
cd C:\Users\ayoub\Desktop\S2\.NET\SAVSolution\SAV.API
dotnet run
# API available at: http://localhost:5050
```

**Terminal 2 - Frontend**
```bash
cd C:\Users\ayoub\Desktop\S2\.NET\SAVSolution\sav-webui
npm start
# App available at: http://localhost:3000
```

**Terminal 3 - Optional: Database**
```bash
# Start SQL Server LocalDB (if needed)
sqllocaldb start mssqllocaldb
```

---

## 📱 Access Points

| Service | URL | Purpose |
|---------|-----|---------|
| **Frontend** | http://localhost:3000 | React SPA |
| **API** | http://localhost:5050 | RESTful API |
| **Swagger** | http://localhost:5050/swagger/index.html | API Documentation |
| **Database** | (localdb)\mssqllocaldb | SQL Server |

---

## 🛠️ Development Commands

### .NET / Backend

```bash
# Navigate to project
cd C:\Users\ayoub\Desktop\S2\.NET\SAVSolution

# Build solution
dotnet build

# Run tests
dotnet test

# Run API
cd SAV.API
dotnet run

# Create migration
dotnet ef migrations add <MigrationName> --project SAV.Infrastructure --startup-project SAV.API

# Update database
dotnet ef database update --project SAV.Infrastructure --startup-project SAV.API

# Remove last migration
dotnet ef migrations remove --project SAV.Infrastructure --startup-project SAV.API

# Generate SQL script
dotnet ef migrations script --from <LastAppliedMigration> --to <NewMigration> --output migration.sql

# Check migrations
dotnet ef migrations list --project SAV.Infrastructure --startup-project SAV.API

# Drop database
dotnet ef database drop --project SAV.Infrastructure --startup-project SAV.API
```

### Node.js / Frontend

```bash
# Navigate to project
cd C:\Users\ayoub\Desktop\S2\.NET\SAVSolution\sav-webui

# Install dependencies
npm install

# Start development server
npm start

# Build for production
npm run build

# Run tests
npm test

# Eject configuration (⚠️ irreversible!)
npm run eject

# Update packages
npm update

# Check for vulnerabilities
npm audit

# Fix vulnerabilities
npm audit fix

# Clear cache
npm cache clean --force
```

---

## 🗄️ Database Commands

### SQL Server Management Studio

```sql
-- Connect to: (localdb)\mssqllocaldb

-- Check database
SELECT * FROM sys.databases WHERE name = 'SAVDb';

-- View tables
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';

-- Check users
SELECT * FROM AspNetUsers;

-- Check roles
SELECT * FROM AspNetRoles;

-- View complaints
SELECT * FROM Complaints;

-- Check migrations history
SELECT * FROM __EFMigrationsHistory;

-- Clear all data (⚠️ careful!)
EXEC sp_MSForEachTable 'DISABLE TRIGGER ALL ON ? ; DELETE FROM ? ; ENABLE TRIGGER ALL ON ?'

-- Reset identity
DBCC CHECKIDENT ('AspNetUsers', RESEED, 0);
```

### PowerShell Commands

```powershell
# Start LocalDB
sqllocaldb start mssqllocaldb

# Stop LocalDB
sqllocaldb stop mssqllocaldb

# List LocalDB instances
sqllocaldb info

# Delete LocalDB (⚠️ careful!)
sqllocaldb delete mssqllocaldb
```

---

## 🧪 Testing Commands

### With cURL

```bash
# Register user
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
	"email": "test@example.com",
	"password": "Password123!",
	"fullName": "Test User"
  }'

# Login
curl -X POST http://localhost:5050/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
	"email": "test@example.com",
	"password": "Password123!"
  }'

# Protected request (with token)
curl -X GET http://localhost:5050/api/complaints \
  -H "Authorization: Bearer <YOUR_TOKEN_HERE>"
```

### With PowerShell

```powershell
# Register
$body = @{
	email = "test@example.com"
	password = "Password123!"
	fullName = "Test User"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5050/api/auth/register" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body

# Login
$loginBody = @{
	email = "test@example.com"
	password = "Password123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5050/api/auth/login" `
  -Method Post `
  -ContentType "application/json" `
  -Body $loginBody

$token = $response.data.accessToken
Write-Output "Token: $token"
```

---

## 📝 File Locations

### Important Files

```
C:\Users\ayoub\Desktop\S2\.NET\SAVSolution\
├── SAV.API/Program.cs                          # API configuration
├── SAV.API/appsettings.json                    # Connection string & JWT settings
├── SAV.Infrastructure/Persistence/SavDbContext.cs   # Database context
├── SAV.Infrastructure/Identity/AuthenticationService.cs  # Auth logic
├── SAV.Domain/Entities/                        # Domain models
├── sav-webui/src/App.tsx                       # React app root
├── sav-webui/src/services/apiClient.ts         # API client
├── sav-webui/.env                              # React env vars (create if needed)
├── IMPLEMENTATION_SUMMARY.md                   # Project overview
├── TESTING_GUIDE.md                            # Testing procedures
└── COMPLETION_REPORT.md                        # Completion details
```

---

## 🔧 Configuration Files

### appsettings.json (Backend)
Location: `SAV.API/appsettings.json`

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SAVDb;Integrated Security=true;"
  },
  "JwtSettings": {
	"Key": "your-secret-key-min-32-characters",
	"Issuer": "SAVApp",
	"Audience": "SAVAppUsers",
	"ExpirationMinutes": 60,
	"RefreshTokenExpirationDays": 7
  }
}
```

### .env (Frontend)
Location: `sav-webui/.env` (create if needed)

```
REACT_APP_API_BASE_URL=http://localhost:5050
REACT_APP_API_TIMEOUT=30000
```

---

## 🐛 Debugging

### View Backend Logs
```bash
# In the terminal where `dotnet run` is executing
# Logs appear automatically during development
```

### View Frontend Logs
```bash
# In the terminal where `npm start` is executing
# Also check browser DevTools (F12)
```

### Debug in VS Code

**Launch Configuration** (`.vscode/launch.json`):
```json
{
  "version": "0.2.0",
  "configurations": [
	{
	  "name": ".NET Core Attach",
	  "type": "coreclr",
	  "request": "attach",
	  "processId": "${command:pickProcess}"
	},
	{
	  "name": "Chrome",
	  "type": "chrome",
	  "request": "launch",
	  "url": "http://localhost:3000",
	  "webRoot": "${workspaceFolder}/sav-webui/src"
	}
  ]
}
```

---

## 🔒 Security Checklist

Before production:

- [ ] Change JWT secret key in appsettings.json
- [ ] Enable HTTPS in launchSettings.json
- [ ] Configure CORS properly (not * in production)
- [ ] Add input validation for all endpoints
- [ ] Implement rate limiting
- [ ] Add request logging
- [ ] Use environment variables for secrets
- [ ] Enable HTTPS redirect
- [ ] Set secure cookies (HttpOnly, Secure, SameSite)
- [ ] Implement request signing
- [ ] Add API versioning
- [ ] Use API gateway in production

---

## 📦 Docker Commands

```bash
# Navigate to solution
cd C:\Users\ayoub\Desktop\S2\.NET\SAVSolution

# Build Docker image
docker build -t sav-api:latest -f SAV.API/Dockerfile .

# Run Docker container
docker run -p 5050:5050 -e "ASPNETCORE_ENVIRONMENT=Development" sav-api:latest

# Docker Compose (if configured)
docker-compose up -d
docker-compose down
docker-compose logs -f

# Clean Docker
docker system prune -a
```

---

## 🌐 Useful URLs

```
# Development
http://localhost:3000          # Frontend
http://localhost:5050          # Backend
http://localhost:5050/swagger  # API docs

# Testing
http://jwt.io                  # JWT decoder
http://postman.com            # API testing tool

# Documentation
https://docs.microsoft.com/dotnet              # .NET docs
https://react.dev                             # React docs
https://entityframeworkcore.com               # EF Core docs
https://learn.microsoft.com/aspnet             # ASP.NET docs
```

---

## 📋 Troubleshooting Commands

```bash
# Clear NuGet cache
nuget locals all -clear

# Clear npm cache
npm cache clean --force

# Remove node_modules and reinstall
cd sav-webui
rm -r node_modules
npm install

# Force rebuild .NET
dotnet clean
dotnet build --force

# Check port usage (Windows)
netstat -ano | findstr :5050
netstat -ano | findstr :3000

# Kill process on port (Windows)
taskkill /PID <PID> /F

# Restart LocalDB
sqllocaldb stop mssqllocaldb
sqllocaldb start mssqllocaldb
```

---

## 📚 Documentation

All documentation is in Markdown format at the root of the solution:

- `README.md` - Quick start guide
- `GETTING_STARTED.md` - Initial setup
- `API_DOCUMENTATION.md` - API reference
- `IMPLEMENTATION_SUMMARY.md` - Project overview
- `TESTING_GUIDE.md` - Testing procedures
- `COMPLETION_REPORT.md` - Completion details
- `QUICK_REFERENCE.md` - This file

---

## 🎯 Common Tasks

### Add a New Entity

1. Create entity in `SAV.Domain/Entities/`
2. Add DbSet to `SavDbContext`
3. Add configuration in `OnModelCreating`
4. Create migration: `dotnet ef migrations add Add<Entity>`
5. Apply: `dotnet ef database update`

### Add a New API Endpoint

1. Create command/query in `SAV.Application/Features/`
2. Create handler for MediatR
3. Create controller in `SAV.API/Controllers/`
4. Add service registration in `Program.cs`
5. Test via Swagger UI

### Add a New React Page

1. Create component in `sav-webui/src/pages/`
2. Add route in `App.tsx`
3. Create service if needed in `src/services/`
4. Import styles and dependencies

---

## ✅ Health Check

```bash
# Is backend running?
curl http://localhost:5050/swagger

# Is frontend running?
curl http://localhost:3000

# Is database connected?
# Check SAV.API logs for connection messages

# Are tokens working?
# Try login in React app and check localStorage
```

---

## 🎓 Learning Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [JWT Authentication](https://tools.ietf.org/html/rfc7519)
- [React Best Practices](https://react.dev/learn)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

---

## 📞 Support

If you encounter issues:

1. **Check logs** in both backend and frontend terminals
2. **Review documentation** in this folder
3. **Check database** connection in SSMS
4. **Test endpoints** in Swagger UI
5. **Debug in browser** using DevTools (F12)

---

## 🎉 You're All Set!

Everything is ready for development. Start with:

```bash
# Terminal 1
cd SAV.API && dotnet run

# Terminal 2
cd sav-webui && npm start

# Open browser
# http://localhost:3000
```

**Happy coding! 🚀**
