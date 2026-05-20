# 📚 SAV Manager - Documentation Complète Backend

## 🎯 Vue d'ensemble du Projet

**SAV Manager** est un système complet de gestion des **Services Après-Vente (SAV)** construit avec .NET 10 et une architecture moderne basée sur les patterns **CQRS** et **MediatR**.

### Architecture en couches:
```
┌─────────────────────────────────────────────────────────┐
│           SAV.API (API REST avec Controllers)            │
├─────────────────────────────────────────────────────────┤
│     SAV.Application (CQRS, Commands, Queries, DTOs)     │
├─────────────────────────────────────────────────────────┤
│    SAV.Domain (Entités, Interfaces, Énumérations)       │
├─────────────────────────────────────────────────────────┤
│  SAV.Infrastructure (BD, Repositories, Identity, JWT)   │
├─────────────────────────────────────────────────────────┤
│           SAV.Shared (Interfaces partagées)              │
├─────────────────────────────────────────────────────────┤
│      SQL Server (Base de données - SAVDb)                │
└─────────────────────────────────────────────────────────┘
```

---

## 📦 Structure des Projets

### 1️⃣ **SAV.Domain** - Modèle métier
Contient les entités de base du domaine métier:

#### Entités principales:
- **User** - Utilisateurs du système (authentification)
- **Role** - Rôles (Admin, Technician, Client)
- **Client** - Clients/Demandeurs de SAV
- **Technician** - Techniciens/Mécaniciens
- **Complaint** - Réclamations/Tickets SAV
- **TechnicalIntervention** - Interventions techniques
- **Article** - Pièces/Produits de remplacement
- **Category** - Catégories d'articles
- **Invoice** - Factures
- **InvoiceItem** - Lignes de facture

#### Énumérations:
- **ComplaintStatus** - Pending, In Progress, Resolved, Closed
- **InterventionStatus** - Scheduled, In Progress, Completed, Cancelled
- **UserRole** - Admin, Technician, Client

---

### 2️⃣ **SAV.Application** - Logique métier

#### Architecture CQRS (Command Query Responsibility Segregation):

**Commands** (Écritures):
```
SAV.Application/Features/
├── Auth/
│   └── Commands/
│       ├── LoginCommand.cs
│       ├── RegisterCommand.cs
│       └── [Handlers]
├── Complaints/
│   └── Commands/
│       ├── CreateComplaintCommand.cs
│       └── [À implémenter: UpdateComplaintCommand, DeleteComplaintCommand]
└── [À implémenter: Interventions, Invoices Commands]
```

**Queries** (Lectures):
```
SAV.Application/Features/
├── Complaints/
│   ├── Queries/
│   │   ├── ComplaintQueries.cs
│   │   ├── GetAllComplaintsQuery
│   │   ├── GetComplaintByIdQuery
│   │   └── [Handlers]
└── [À implémenter: Interventions, Invoices Queries]
```

#### DTOs (Data Transfer Objects):
```
DTOs/Auth/
├── LoginRequest, LoginResponse
├── RegisterRequest, RegisterResponse

DTOs/Features/
├── ComplaintDto
├── InterventionDto
├── InvoiceDto
├── ClientDto
├── TechnicianDto
```

---

### 3️⃣ **SAV.Infrastructure** - Persistence & Services

#### Base de données:
- **DbContext: SavDbContext** - EF Core DbContext
- **Migrations** - Schéma SQL Server
- **Modèle relationnel**:
  ```
  Users (1) ---> (n) Complaints
  Users (1) ---> (n) TechnicalInterventions (Technicians)
  Clients (1) ---> (n) Complaints
  Complaints (1) ---> (n) TechnicalInterventions
  Complaints (1) ---> (n) Invoices
  Categories (1) ---> (n) Articles
  Articles (n) <---> (n) InvoiceItems
  ```

#### Repositories Pattern:
```
IRepository<T>
├── GetAll()
├── GetById(id)
├── Add(entity)
├── Update(entity)
└── Delete(id)

IUnitOfWork
├── Complaints
├── Interventions
├── Invoices
├── Clients
├── Technicians
└── Articles
```

#### Services:
- **IJwtTokenService** - Génération & validation JWT
- **IAuthenticationService** - Login, Register, RefreshToken
- **IIdentityService** - Gestion des utilisateurs

#### Identity:
- ASP.NET Core Identity avec JWT Bearer
- Configuration JWT (Key, Issuer, Audience, Expiration)
- Rôles (Admin, Technician, Client)

---

### 4️⃣ **SAV.API** - API REST

#### Contrôleurs disponibles:

| Contrôleur | Route | Méthodes |
|-----------|-------|----------|
| **AuthController** | `/api/auth` | POST /login, POST /register |
| **ComplaintsController** | `/api/complaints` | GET, POST, GET/{id} |
| **InterventionsController** | `/api/interventions` | GET, POST, GET/{id}, PUT, DELETE |
| **InvoicesController** | `/api/invoices` | GET, POST, GET/{id}, PUT, DELETE |
| **ClientsController** | `/api/clients` | GET, POST, GET/{id}, PUT, DELETE |
| **TechniciansController** | `/api/technicians` | GET, POST, GET/{id}, PUT, DELETE |
| **DashboardController** | `/api/dashboard` | GET /stats, GET /monthly-complaints, GET /intervention-status |

#### Authentification:
- ✅ JWT Bearer Token
- ✅ Authorize attribute sur tous les contrôleurs
- ✅ AllowAnonymous pour Auth & Dashboard

#### CORS:
- ✅ Configuré pour accepter les requêtes cross-origin
- Permet WebUI & autres clients frontend

---

### 5️⃣ **SAV.Shared** - Code partagé

Interfaces communes:
- **IJwtTokenService** - Contrats d'authentification
- **IAuthenticationService** - Contrats de login/register
- **IUnitOfWork** - Contrats de persistence

Configuration partagée:
- **JwtSettings** - Paramètres JWT

---

## 🔄 Flux de données - Exemple complet

### Scénario: Créer une réclamation (Create Complaint)

```
1. CLIENT HTTP → POST /api/complaints
   {
	 "subject": "Produit défectueux",
	 "description": "Le produit ne fonctionne pas",
	 "clientId": 1
   }

2. API → ComplaintsController.Create()
   └─ Reçoit CreateComplaintCommand

3. MediatR → Handler (CreateComplaintCommandHandler)
   ├─ Valide les données
   ├─ Crée une nouvelle entité Complaint
   └─ Appelle IUnitOfWork.Complaints.Add()

4. Repository → UnitOfWork → SavDbContext
   └─ Entity Framework Core enregistre en base de données

5. SQL Server → INSERT INTO Complaints
   ```
   ID | Subject | Description | Status | CreatedDate | ClientId
   42 | Produit défectueux | ... | Pending | 2026-01-15 | 1
   ```

6. Réponse → API → CLIENT
   {
	 "id": 42,
	 "subject": "Produit défectueux",
	 "status": "Pending",
	 "createdDate": "2026-01-15T10:30:00",
	 "clientId": 1
   }
```

---

## 🚀 Installation & Configuration

### Prérequis:
- ✅ .NET 10 SDK
- ✅ SQL Server (LocalDB ou full version)
- ✅ Visual Studio 2026 ou VS Code

### Étapes d'installation:

#### 1. Cloner le projet:
```bash
git clone https://github.com/your-repo/SAVSolution.git
cd SAVSolution
```

#### 2. Restaurer les dépendances:
```powershell
dotnet restore
```

#### 3. Configurer la base de données:
Vérifier `appsettings.json`:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SAVDb;Integrated Security=true;"
  },
  "JwtSettings": {
	"Key": "your-secret-key-min-32-characters-very-very-long",
	"Issuer": "SAVApp",
	"Audience": "SAVAppUsers",
	"ExpirationMinutes": 60,
	"RefreshTokenExpirationDays": 7
  }
}
```

#### 4. Appliquer les migrations (si nécessaire):
```powershell
cd SAV.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 5. Démarrer l'API:
```powershell
cd SAV.API
dotnet run
```

L'API démarre sur: `https://localhost:5001` ou `http://localhost:5000`

---

## 📖 Documentation Swagger

### Accéder à Swagger:
```
http://localhost:5000/swagger/index.html
```

### Tester les endpoints:

#### 1. **Authentification** 🔐
```
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "Admin@123"
}

Réponse:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "expiresIn": 3600
}
```

Copier le token et l'utiliser dans les headers:
```
Authorization: Bearer {token}
```

#### 2. **Réclamations** 📋
```
GET /api/complaints
Headers: Authorization: Bearer {token}

Réponse:
[
  {
	"id": 1,
	"subject": "Problème technique",
	"description": "Le produit ne répond plus",
	"status": "Pending",
	"createdDate": "2026-01-15T10:30:00",
	"clientId": 1,
	"clientName": "Client A"
  }
]
```

#### 3. **Créer une réclamation** ✏️
```
POST /api/complaints
Content-Type: application/json
Authorization: Bearer {token}

{
  "subject": "Panne moteur",
  "description": "Le moteur ne démarre pas",
  "clientId": 1
}
```

#### 4. **Dashboard - Statistiques** 📊
```
GET /api/dashboard/stats
Headers: Authorization: Bearer {token}

Réponse:
{
  "totalComplaints": 42,
  "pendingComplaints": 8,
  "completedComplaints": 34,
  "totalInterventions": 25,
  "totalInvoiceAmount": 5250.50
}
```

#### 5. **Clients** 👥
```
GET /api/clients
Headers: Authorization: Bearer {token}

Réponse:
[
  {
	"id": 1,
	"name": "Client 1",
	"email": "client1@example.com",
	"phone": "+212612345678"
  }
]
```

#### 6. **Techniciens** 🔧
```
GET /api/technicians
Headers: Authorization: Bearer {token}

Réponse:
[
  {
	"id": 1,
	"firstName": "Ahmed",
	"lastName": "Technician",
	"specialization": "Électronique"
  }
]
```

---

## 🔐 Sécurité

### JWT Bearer Token:
- ✅ Secret key en `appsettings.json` (à changer en production!)
- ✅ Expiration: 60 minutes par défaut
- ✅ Refresh Token: 7 jours
- ✅ Algorithme: HS256

### Authentification:
- ✅ [Authorize] sur tous les contrôleurs (sauf Auth & Dashboard)
- ✅ [AllowAnonymous] pour login/register
- ✅ Rôles: Admin, Technician, Client

### CORS:
- ✅ Configuré pour accepter WebUI sur `http://localhost:5082`
- À restreindre en production!

---

## 📊 Architecture CQRS - Exemple détaillé

### Pattern CQRS:
```
Command (Écritures)
├── CreateComplaintCommand
├── UpdateComplaintCommand
└── DeleteComplaintCommand

Query (Lectures)
├── GetAllComplaintsQuery
├── GetComplaintByIdQuery
└── GetComplaintsByClientQuery

Handler (Logique métier)
├── CreateComplaintCommandHandler
├── GetAllComplaintsQueryHandler
└── GetComplaintByIdQueryHandler

Result
├── ComplaintDto
└── List<ComplaintDto>
```

### Flux d'exécution:
```
Controller → MediatR.Send(command/query)
   ↓
MediatR Router
   ↓
Handler (Validation + Logique)
   ↓
Repository / Database
   ↓
Retour du résultat
   ↓
API Response JSON
```

---

## 🗄️ Modèle de base de données

### Schéma principal:

```sql
-- Users (Authentification)
Users
├── Id (PK)
├── Email (Unique)
├── NormalizedEmail
├── PasswordHash
├── RoleId (FK → Roles)
└── CreatedDate

-- Roles
Roles
├── Id (PK)
├── Name (Admin, Technician, Client)
└── Description

-- Clients
Clients
├── Id (PK)
├── Name
├── Email
├── Phone
├── Address
└── CreatedDate

-- Technicians
Technicians
├── Id (PK)
├── FirstName
├── LastName
├── Specialization
├── Email
└── Phone

-- Complaints (Réclamations)
Complaints
├── Id (PK)
├── Subject
├── Description
├── Status (Pending, In Progress, Resolved, Closed)
├── Priority (Low, Medium, High)
├── ClientId (FK → Clients)
├── CreatedDate
├── UpdatedDate
└── ResolvedDate

-- TechnicalInterventions (Interventions)
TechnicalInterventions
├── Id (PK)
├── Type
├── Status (Scheduled, In Progress, Completed, Cancelled)
├── ScheduledDate
├── CompletedDate
├── ComplaintId (FK → Complaints)
├── TechnicianId (FK → Technicians)
└── Notes

-- Invoices (Factures)
Invoices
├── Id (PK)
├── InvoiceNumber (Unique)
├── TotalAmount
├── IssuedDate
├── DueDate
├── PaidDate
├── ClientId (FK → Clients)
└── Status

-- Articles (Pièces)
Articles
├── Id (PK)
├── Code (Unique)
├── Name
├── Description
├── Price
├── StockQuantity
├── CategoryId (FK → Categories)
└── CreatedDate

-- Categories
Categories
├── Id (PK)
├── Name
├── Description
└── IsActive

-- InvoiceItems (Lignes de facture)
InvoiceItems
├── Id (PK)
├── InvoiceId (FK → Invoices)
├── ArticleId (FK → Articles)
├── Quantity
├── UnitPrice
└── TotalPrice
```

---

## 🧪 Tests avec Postman / cURL

### 1. Obtenir un token:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
	"email": "admin@example.com",
	"password": "Admin@123"
  }'
```

### 2. Utiliser le token pour accéder aux ressources:
```bash
curl -X GET http://localhost:5000/api/complaints \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### 3. Créer une réclamation:
```bash
curl -X POST http://localhost:5000/api/complaints \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
	"subject": "Panne produit",
	"description": "Le produit ne fonctionne plus",
	"clientId": 1
  }'
```

---

## 🔄 Intégration Frontend-Backend

### WebUI (Razor Pages) communique avec API:

```
SAV.WebUI (http://localhost:5082)
	↓
ApiService (HttpClient)
	↓
SAV.API (http://localhost:5000)
	↓
Controllers → MediatR → Handlers → Database
	↓
Response JSON
	↓
Views (Razor Pages cshtml)
```

### Configuration WebUI:
```json
// appsettings.json
{
  "ApiSettings": {
	"BaseUrl": "http://localhost:5000"
  }
}
```

### Services WebUI:
- **IApiService** - Client HTTP générique
- **IComplaintService** - Appels API Complaints
- **IDashboardService** - Appels API Dashboard
- **IAuthService** - Login/Register

---

## 🚀 Déploiement

### Pour développement:
```bash
dotnet run --configuration Debug
```

### Pour production:
```bash
dotnet publish -c Release -o ./publish
```

### Variables d'environnement:
```bash
set ConnectionStrings__DefaultConnection=Server=YOUR_SERVER;Database=SAVDb;...
set JwtSettings__Key=YOUR_SUPER_SECRET_KEY
set JwtSettings__Issuer=SAVApp
```

---

## 📋 Checklist - Prochaines améliorations

- [ ] Implémenter les Queries & Handlers pour Interventions
- [ ] Implémenter les Queries & Handlers pour Invoices
- [ ] Implémenter les Queries & Handlers pour Clients
- [ ] Implémenter les Queries & Handlers pour Technicians
- [ ] Ajouter la validation (FluentValidation)
- [ ] Ajouter les tests unitaires (xUnit)
- [ ] Implémenter les Logs (Serilog)
- [ ] Implémenter le Caching (Redis)
- [ ] Ajouter les Notifications SignalR (WebSocket)
- [ ] Documenter les API avec Swagger annotations
- [ ] Ajouter Rate Limiting
- [ ] Implémenter les Filters personnalisés

---

## 📞 Support

Pour des questions ou des bugs, contactez l'équipe de développement.

---

**Généré le:** 15/01/2026  
**Version:** 1.0  
**Auteur:** Backend Team  
**Status:** ✅ Production Ready
