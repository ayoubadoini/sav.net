# SAV (Service Après-Vente) Manager Application

Professional Service Management Application built with ASP.NET Core 10, React, and SQL Server.

## 📋 Overview

SAV Manager is a complete Service After-Sales (SAV) management system designed for companies specializing in sanitaryware and central heating systems. It provides comprehensive tools for managing clients, complaints, technical interventions, and invoicing.

## ✨ Features

### Core Features
- **Client Management**: Full CRUD operations for customer profiles
- **Complaint Management**: Track and manage customer complaints with status updates
- **Technical Interventions**: Schedule and manage technical service interventions
- **Warranty Management**: Automatic warranty status checking for articles
- **Invoice Generation**: Create and manage invoices with PDF export
- **User Authentication**: Secure JWT-based authentication with refresh tokens
- **Role-Based Access**: Admin, SAV Manager, and Client roles

### Advanced Features
- **Real-time Notifications**: SignalR for live updates
- **File Management**: Upload and manage complaint attachments
- **PDF Export**: Generate invoices and reports
- **Responsive UI**: Bootstrap 5 for mobile-friendly interface
- **API Documentation**: Swagger/OpenAPI integration
- **Docker Support**: Complete containerization setup

## 🏗️ Architecture

### Backend Structure (Clean Architecture)

```
SAV.Domain/
├── Entities/          # Business entities
├── Enums/            # Domain enumerations
└── Interfaces/       # Domain interfaces

SAV.Application/
├── Features/         # CQRS commands and queries
├── DTOs/            # Data transfer objects
├── Validators/      # FluentValidation rules
└── Handlers/        # MediatR handlers

SAV.Infrastructure/
├── Persistence/     # EF Core DbContext
├── Repositories/    # Repository pattern
├── Identity/        # JWT & Authentication
└── Services/        # Business services

SAV.API/
├── Controllers/     # REST endpoints
├── Hubs/           # SignalR hubs
└── Middleware/     # Custom middleware

SAV.Shared/
└── Utilities/      # Shared utilities
```

### Frontend Structure (React + TypeScript)

```
sav-webui/
├── src/
│   ├── components/   # Reusable React components
│   ├── pages/       # Page components
│   ├── services/    # API and business logic services
│   ├── styles/      # Global styles
│   └── App.tsx      # Root component
```

## 🚀 Quick Start

### Prerequisites
- .NET 10 SDK
- Node.js 18+
- SQL Server 2022 (or LocalDB)
- Docker & Docker Compose (optional)

### Installation

1. **Clone the repository**
```bash
git clone <repository-url>
cd SAVSolution
```

2. **Setup Database**
```bash
dotnet ef database update --project SAV.Infrastructure --startup-project SAV.API
```

3. **Configure JWT Settings**
Update `appsettings.json` in SAV.API with your JWT secret:
```json
{
  "JwtSettings": {
	"Key": "your-secret-key-min-32-characters-here-for-jwt",
	"Issuer": "SAVApp",
	"Audience": "SAVAppUsers",
	"ExpirationMinutes": 60,
	"RefreshTokenExpirationDays": 7
  }
}
```

4. **Run Backend API**
```bash
cd SAV.API
dotnet run
```

API will be available at: `https://localhost:7001`
Swagger UI: `https://localhost:7001/swagger`

5. **Run Frontend**
```bash
cd sav-webui
npm install
npm start
```

Frontend will be available at: `http://localhost:3000`

## 🐳 Docker Setup

### Using Docker Compose

```bash
docker-compose up -d
```

Services will be available at:
- API: `https://localhost:7001`
- Web: `http://localhost:3000`
- SQL Server: `localhost:1433`

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/refresh-token` - Refresh JWT token
- `POST /api/auth/logout` - Logout user

### Complaints
- `GET /api/complaints` - Get all complaints
- `GET /api/complaints/{id}` - Get complaint by ID
- `POST /api/complaints` - Create new complaint
- `PUT /api/complaints/{id}` - Update complaint

### Interventions
- `GET /api/interventions` - Get all interventions
- `GET /api/interventions/{id}` - Get intervention by ID
- `POST /api/interventions` - Schedule intervention
- `PUT /api/interventions/{id}` - Update intervention

### Clients
- `GET /api/clients` - Get all clients
- `GET /api/clients/{id}` - Get client by ID
- `POST /api/clients` - Create new client
- `PUT /api/clients/{id}` - Update client

## 🔐 Authentication

The application uses JWT (JSON Web Tokens) for authentication:

1. User registers or logs in
2. API returns access token and refresh token
3. Client stores tokens in localStorage
4. Axios interceptor adds token to all requests
5. Refresh token used to get new access token when expired

### User Roles

- **Admin**: Full system access
- **ResponsableSAV**: Can manage clients, complaints, and interventions
- **Client**: Can view own profile, complaints, and interventions

## 📦 Technologies Used

### Backend
- **Framework**: ASP.NET Core 10
- **ORM**: Entity Framework Core
- **Database**: SQL Server
- **Authentication**: ASP.NET Identity + JWT
- **API**: RESTful with Swagger/OpenAPI
- **Real-time**: SignalR
- **Validation**: FluentValidation
- **CQRS**: MediatR
- **Logging**: Serilog
- **PDF**: iTextSharp
- **Containerization**: Docker

### Frontend
- **Framework**: React 18 with TypeScript
- **HTTP Client**: Axios
- **Routing**: React Router v6
- **Styling**: Bootstrap 5
- **Real-time**: @microsoft/signalr
- **State Management**: React Hooks

## 🔄 CI/CD Pipeline

GitHub Actions workflow automatically:
- Builds .NET projects on push
- Runs unit tests
- Builds Docker images
- Deploys to production (on main branch)

See `.github/workflows/ci-cd.yml` for details.

## 📊 Database Schema

### Main Tables
- **Users**: User accounts with authentication
- **Roles**: Application roles
- **Clients**: Customer information
- **Articles**: Products/items owned by clients
- **Categories**: Product categories
- **Complaints**: Customer complaints
- **TechnicalInterventions**: Service interventions
- **Invoices**: Generated invoices
- **InvoiceItems**: Invoice line items
- **Technicians**: Service technician information

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=...;Database=SAVDb;..."
  },
  "JwtSettings": {
	"Key": "...",
	"Issuer": "SAVApp",
	"Audience": "SAVAppUsers",
	"ExpirationMinutes": 60
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information"
	}
  }
}
```

## 📝 Environment Variables

Create `.env` file in root:
```
DB_CONNECTION_STRING=Server=localhost;Database=SAVDb;...
JWT_SECRET=your-secret-key-min-32-characters
JWT_ISSUER=SAVApp
JWT_AUDIENCE=SAVAppUsers
REACT_APP_API_URL=https://localhost:7001/api
```

## 🧪 Testing

Run unit tests:
```bash
dotnet test
```

## 🎨 UI Components

### Available Components
- **NavBar**: Navigation with authentication status
- **PrivateRoute**: Protected route wrapper
- **FileUploadComponent**: File upload with validation
- **ComplaintsPage**: Complaint listing and management
- **DashboardPage**: KPI cards and statistics

## 📱 Responsive Design

The application is fully responsive with Bootstrap 5:
- Mobile: < 576px
- Tablet: 576px - 768px
- Desktop: > 768px

## 🐛 Error Handling

- Global exception handling middleware
- Structured error responses
- Client-side error boundaries
- Automatic token refresh on 401 errors

## 🚀 Performance Optimizations

- Database query optimization with EF Core
- Lazy loading prevention
- Client-side pagination
- Response caching
- SignalR for real-time updates (reduces polling)

## 📖 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [React Documentation](https://react.dev)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [JWT.io](https://jwt.io)

## 🤝 Contributing

1. Create a feature branch
2. Make changes
3. Commit with clear messages
4. Push to GitHub
5. Create Pull Request

## 📄 License

MIT License - See LICENSE file for details

## 👨‍💼 Support

For issues and questions, please create an issue in the repository.

## 🎯 Roadmap

- [ ] Multi-language support
- [ ] Advanced reporting and analytics
- [ ] Mobile app (React Native)
- [ ] SMS notifications
- [ ] Automated invoice sending
- [ ] Payment gateway integration
- [ ] WhatsApp integration
- [ ] Machine learning for complaint classification

---

**Version**: 1.0.0  
**Last Updated**: 2026-01-17  
**Status**: Production Ready
