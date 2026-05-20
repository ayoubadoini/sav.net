# SAV Application - Getting Started Guide

## 🎯 First Steps

### 1. Database Setup

Before running the application, you need to create the database:

```bash
# From SAV.API directory
dotnet ef database update --project SAV.Infrastructure
```

This will:
- Create the SAVDb database in SQL Server
- Run all migrations
- Seed initial roles (Admin, ResponsableSAV, Client)

### 2. Create Test Accounts

Use the API endpoints to create test accounts:

**Register as Admin:**
```bash
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
	"email": "admin@sav.com",
	"password": "Admin123!",
	"fullName": "Admin User"
  }'
```

**Register as Client:**
```bash
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
	"email": "client@example.com",
	"password": "Client123!",
	"fullName": "John Doe"
  }'
```

**Login:**
```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
	"email": "admin@sav.com",
	"password": "Admin123!"
  }'
```

### 3. Swagger API Documentation

Access Swagger UI at: `https://localhost:7001/swagger`

You can:
- View all endpoints
- Test endpoints directly
- See request/response schemas
- Copy curl commands

### 4. Frontend Development

```bash
cd sav-webui

# Install dependencies
npm install

# Start development server
npm start

# Build for production
npm run build

# Run tests
npm test
```

## 🔑 Important Configuration Files

### SAV.API/appsettings.json
- Database connection string
- JWT settings (secret key, expiration)
- Logging configuration
- Cors settings

### sav-webui/.env
```
REACT_APP_API_URL=https://localhost:7001/api
```

## 📝 Common Tasks

### Add a New Feature

1. **Create Entity** in `SAV.Domain/Entities/`
2. **Add DbSet** to `SavDbContext`
3. **Create Migration**: `dotnet ef migrations add FeatureName`
4. **Create DTOs** in `SAV.Application/Features/FeatureName/DTOs/`
5. **Create Commands/Queries** in `SAV.Application/Features/FeatureName/`
6. **Create Handlers** in `SAV.Application/Features/FeatureName/Handlers/`
7. **Create Controller** in `SAV.API/Controllers/`
8. **Create React Component** in `sav-webui/src/`

### Database Migration

```bash
# Add new migration
dotnet ef migrations add MigrationName --project SAV.Infrastructure

# Update database
dotnet ef database update --project SAV.Infrastructure

# Rollback last migration
dotnet ef migrations remove --project SAV.Infrastructure
```

### Running Locally

**Terminal 1 - API:**
```bash
cd SAV.API
dotnet run
```

**Terminal 2 - Frontend:**
```bash
cd sav-webui
npm start
```

## 🐳 Docker Development

### Build Images
```bash
docker-compose build
```

### Start Services
```bash
docker-compose up -d
```

### View Logs
```bash
docker-compose logs -f api
docker-compose logs -f web
docker-compose logs -f sqlserver
```

### Stop Services
```bash
docker-compose down
```

## 🔍 Debugging

### Backend Debug
Visual Studio debug mode:
```
F5 in Visual Studio
```

VS Code launch configuration:
```json
{
	"version": "0.2.0",
	"configurations": [
		{
			"name": ".NET Core Launch (web)",
			"type": "coreclr",
			"request": "launch",
			"preLaunchTask": "build",
			"program": "${workspaceFolder}/SAV.API/bin/Debug/net10.0/SAV.API.dll",
			"args": [],
			"cwd": "${workspaceFolder}/SAV.API",
			"stopAtEntry": false,
			"serverReadyAction": {
				"pattern": "\\bNow listening on:\\s+(https?://\\S+)",
				"uriFormat": "{0}",
				"action": "openExternally"
			}
		}
	]
}
```

### Frontend Debug
Chrome DevTools:
- Open: `http://localhost:3000`
- Press `F12` for Developer Tools
- React DevTools extension recommended

## ⚠️ Common Issues

### Database Connection Error
- Check SQL Server is running
- Verify connection string in appsettings.json
- Check port 1433 is available

### JWT Token Expired
- Client automatically refreshes token
- Check JWT secret matches API

### CORS Error
- Verify CORS policy in Program.cs
- Check API and React URLs match

### Port Already in Use
- API: Change port in launchSettings.json
- React: `PORT=3001 npm start`

## 📚 Project Structure

```
SAVSolution/
├── SAV.Domain/              # Business entities & rules
├── SAV.Application/         # CQRS, validators, DTOs
├── SAV.Infrastructure/      # EF Core, repositories, services
├── SAV.API/                 # REST API controllers
├── SAV.Shared/              # Shared utilities
├── sav-webui/               # React frontend
├── Dockerfile               # API container
├── docker-compose.yml       # Multi-container setup
├── .github/workflows/       # CI/CD pipelines
└── README.md               # Main documentation
```

## 🚀 Deployment Checklist

- [ ] Update appsettings.Production.json
- [ ] Set strong JWT secret
- [ ] Configure database connection
- [ ] Set CORS origins properly
- [ ] Enable HTTPS
- [ ] Setup logging (Serilog)
- [ ] Test all endpoints
- [ ] Build Docker images
- [ ] Run CI/CD pipeline
- [ ] Monitor application logs

## 📞 Support

For help:
1. Check README.md for architecture overview
2. Review existing issues on GitHub
3. Check API documentation in Swagger
4. Enable debug logging in Program.cs

Happy coding! 🎉
