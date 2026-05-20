# 🏢 SAV Manager - Enterprise Features Documentation

## Table des Matières
1. [Role-Based Authorization](#role-based-authorization)
2. [Audit Fields System](#audit-fields-system)
3. [Architecture & Design](#architecture--design)
4. [Implémentation](#implémentation)
5. [Exemples d'utilisation](#exemples-dutilisation)
6. [Best Practices](#best-practices)

---

## Role-Based Authorization

### Vue d'ensemble
Système d'autorisation complète basé sur les **rôles JWT** et les **policies ASP.NET Core**.

### Rôles disponibles
```csharp
public const string Admin = "Admin";           // Accès complet
public const string Technician = "Technician"; // Opérations techniques
public const string Client = "Client";          // Opérations clients
```

### Policies disponibles
```csharp
AdminOnly          → Uniquement les Admins
TechnicianOnly     → Uniquement les Technicians
ClientOnly         → Uniquement les Clients
AdminOrTechnician  → Admin OU Technician
AdminOrClient      → Admin OU Client
Authenticated      → Tous les utilisateurs authentifiés
```

### Utilisation dans les contrôleurs

#### ❌ Avant (Sans autorisation)
```csharp
[ApiController]
[Route("api/[controller]")]
public class ComplaintsController : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult> GetAll()
	{
		// N'importe qui peut accéder
		return Ok();
	}
}
```

#### ✅ Après (Avec autorisation)
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplaintsController : ControllerBase
{
	private readonly ICurrentUserService _currentUserService;

	public ComplaintsController(ICurrentUserService currentUserService)
	{
		_currentUserService = currentUserService;
	}

	// Tous les authentifiés peuvent lire
	[HttpGet]
	[Authorize]
	public async Task<ActionResult> GetAll()
	{
		return Ok();
	}

	// Uniquement les Clients peuvent créer
	[HttpPost]
	[Authorize(Policy = "ClientOnly")]
	public async Task<ActionResult> Create([FromBody] CreateComplaintCommand command)
	{
		var userId = _currentUserService.GetCurrentUserId();
		var role = _currentUserService.GetCurrentUserRole();

		// Vérifier que le client crée pour lui-même
		if (command.ClientId != userId)
			return Forbid();

		return CreatedAtAction(nameof(GetById), new { id = 1 }, result);
	}

	// Admin OU Technician peuvent mettre à jour
	[HttpPut("{id}")]
	[Authorize(Policy = "AdminOrTechnician")]
	public async Task<ActionResult> Update(int id, [FromBody] UpdateCommand cmd)
	{
		return Ok();
	}

	// Uniquement Admin peut supprimer
	[HttpDelete("{id}")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<ActionResult> Delete(int id)
	{
		return NoContent();
	}
}
```

### Utilisation de ICurrentUserService

```csharp
public class MyService
{
	private readonly ICurrentUserService _currentUserService;

	public MyService(ICurrentUserService currentUserService)
	{
		_currentUserService = currentUserService;
	}

	public void DoSomething()
	{
		// Récupérer l'utilisateur courant
		int? userId = _currentUserService.GetCurrentUserId();
		string? email = _currentUserService.GetCurrentUserEmail();
		string? fullName = _currentUserService.GetCurrentUserFullName();
		string? role = _currentUserService.GetCurrentUserRole();

		// Vérifier les rôles
		if (_currentUserService.IsAdmin())
		{
			// Admin operations
		}

		if (_currentUserService.IsTechnician())
		{
			// Technician operations
		}

		if (_currentUserService.IsClient())
		{
			// Client operations
		}

		// Vérifier plusieurs rôles
		if (_currentUserService.HasAnyRole(Roles.Admin, Roles.Technician))
		{
			// Admin or Technician operations
		}
	}
}
```

### Configuration dans Program.cs

```csharp
// Services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Policies
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy(Policies.AdminOnly, policy =>
		policy.RequireRole(Roles.Admin));

	options.AddPolicy(Policies.AdminOrTechnician, policy =>
		policy.RequireRole(Roles.Admin, Roles.Technician));

	// ... autres policies
});
```

---

## Audit Fields System

### Vue d'ensemble
Système d'audit automatique qui track:
- **CreatedBy** - User ID qui a créé l'entité
- **CreatedDate** - Date/heure (UTC) de création
- **UpdatedBy** - User ID qui a modifié l'entité
- **UpdatedDate** - Date/heure (UTC) de dernière modification
- **IsDeleted** - Flag pour soft delete
- **DeletedDate** - Date/heure (UTC) de suppression

### Architecture

#### 1. BaseAuditableEntity
```csharp
public abstract class BaseAuditableEntity
{
	public int Id { get; set; }
	public int? CreatedBy { get; set; }
	public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
	public int? UpdatedBy { get; set; }
	public DateTime? UpdatedDate { get; set; }
	public bool IsDeleted { get; set; } = false;
	public DateTime? DeletedDate { get; set; }
}
```

#### 2. Entités héritant
```csharp
public class Complaint : BaseAuditableEntity
{
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	// ...
}

public class Invoice : BaseAuditableEntity
{
	public string InvoiceNumber { get; set; } = string.Empty;
	// ...
}

// Toutes les entités héritent de BaseAuditableEntity:
// - Complaint
// - TechnicalIntervention
// - Invoice
// - Client
// - Technician
// - Article
// - Category
```

#### 3. SaveChangesAsync Override
```csharp
public class SavDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		ApplyAuditingInfo();
		return await base.SaveChangesAsync(cancellationToken);
	}

	private void ApplyAuditingInfo()
	{
		var userId = _currentUserService?.GetCurrentUserId();
		var now = DateTime.UtcNow;

		var entries = ChangeTracker.Entries<BaseAuditableEntity>();

		foreach (var entry in entries)
		{
			switch (entry.State)
			{
				case EntityState.Added:
					// On create
					entry.Entity.CreatedBy = userId;
					entry.Entity.CreatedDate = now;
					break;

				case EntityState.Modified:
					// On update - protect CreatedBy and CreatedDate
					entry.Property(e => e.CreatedBy).IsModified = false;
					entry.Property(e => e.CreatedDate).IsModified = false;

					entry.Entity.UpdatedBy = userId;
					entry.Entity.UpdatedDate = now;
					break;

				case EntityState.Deleted:
					// Soft delete
					entry.State = EntityState.Modified;
					entry.Entity.IsDeleted = true;
					entry.Entity.DeletedDate = now;
					entry.Entity.UpdatedBy = userId;
					entry.Entity.UpdatedDate = now;
					break;
			}
		}
	}
}
```

### Utilisation automatique

```csharp
// CRÉER une réclamation
var complaint = new Complaint
{
	Title = "Produit défectueux",
	Description = "Le produit ne fonctionne pas",
	ClientId = 1,
	ArticleId = 1
};

_context.Complaints.Add(complaint);
await _context.SaveChangesAsync();

// Résultat (automatique):
// complaint.CreatedBy = 42  (User ID courant)
// complaint.CreatedDate = 2026-01-15 10:30:00 (UTC)
// complaint.UpdatedBy = null
// complaint.UpdatedDate = null
// complaint.IsDeleted = false


// MODIFIER la réclamation
complaint.Title = "Produit très défectueux";
_context.Complaints.Update(complaint);
await _context.SaveChangesAsync();

// Résultat (automatique):
// complaint.CreatedBy = 42  (inchangé - protégé)
// complaint.CreatedDate = 2026-01-15 10:30:00 (inchangé - protégé)
// complaint.UpdatedBy = 42  (User ID courant)
// complaint.UpdatedDate = 2026-01-15 11:45:00 (UTC maintenant)


// SUPPRIMER la réclamation (soft delete)
_context.Complaints.Remove(complaint);
await _context.SaveChangesAsync();

// Résultat (automatique - SOFT DELETE):
// complaint.IsDeleted = true
// complaint.DeletedDate = 2026-01-15 12:00:00 (UTC)
// complaint.UpdatedBy = 42
// complaint.UpdatedDate = 2026-01-15 12:00:00

// Note: Dans les requêtes, les entités supprimées sont automatically exclues
// grâce aux Query Filters
```

### Query Filters (Soft Delete)

```csharp
// Configuration dans OnModelCreating
modelBuilder.Entity<Complaint>()
	.HasQueryFilter(c => !c.IsDeleted);

modelBuilder.Entity<TechnicalIntervention>()
	.HasQueryFilter(ti => !ti.IsDeleted);

// Utilisation automatique:
var activeComplaints = await _context.Complaints.ToListAsync();
// Retourne UNIQUEMENT les entités où IsDeleted = false
// Les entités supprimées sont automatically exclues
```

### Indexes pour performance

```csharp
// Indexes créés automatiquement dans la migration
modelBuilder.Entity<Complaint>().HasIndex(c => c.IsDeleted);
modelBuilder.Entity<Complaint>().HasIndex(c => c.CreatedBy);
modelBuilder.Entity<Invoice>().HasIndex(i => i.IsDeleted);
// ... etc

// Améliore performance des queries avec soft delete
```

---

## Architecture & Design

### Clean Architecture Layers

```
┌─────────────────────────────────────────────┐
│           SAV.API (Controllers)              │
│  [Authorize(Policy = "...")]               │
│  Injection: ICurrentUserService            │
└─────────────────────────────────────────────┘
					 ↓
┌─────────────────────────────────────────────┐
│        SAV.Application (CQRS)               │
│  Commands, Queries, Handlers               │
│  Logique métier                            │
└─────────────────────────────────────────────┘
					 ↓
┌─────────────────────────────────────────────┐
│         SAV.Domain (Entities)               │
│  BaseAuditableEntity                       │
│  IAuditableEntity, IAuditable              │
└─────────────────────────────────────────────┘
					 ↓
┌─────────────────────────────────────────────┐
│     SAV.Infrastructure (Persistence)        │
│  SavDbContext (SaveChangesAsync override)   │
│  CurrentUserService                        │
│  JwtTokenService (avec role claims)        │
│  Repositories, UnitOfWork                  │
└─────────────────────────────────────────────┘
					 ↓
┌─────────────────────────────────────────────┐
│    SAV.Shared (Interfaces & Constants)      │
│  ICurrentUserService                       │
│  Roles, Policies, CustomClaimTypes         │
└─────────────────────────────────────────────┘
					 ↓
┌─────────────────────────────────────────────┐
│        SQL Server (Database)                │
│  Audit fields indexées et filtrées         │
└─────────────────────────────────────────────┘
```

### Principes appliqués

✅ **SOLID Principles**
- Single Responsibility: Classes ont une seule responsabilité
- Open/Closed: Extensible via policies
- Liskov Substitution: BaseAuditableEntity substitutable
- Interface Segregation: ICurrentUserService, IAuditableEntity
- Dependency Inversion: Interfaces injectées

✅ **Clean Code**
- Zéro magic strings (constantes centralisées)
- Naming cohérent
- Documentation XML complète
- Separation of concerns

✅ **Enterprise Patterns**
- Policy-based authorization
- Automatic audit tracking
- Soft delete
- Role-based access control (RBAC)
- JWT with custom claims
- Unit of Work pattern

---

## Implémentation

### Fichiers créés/modifiés

```
✅ SAV.Domain/Common/
   ├── BaseAuditableEntity.cs (NEW)
   └── IAuditableEntity.cs (NEW)

✅ SAV.Domain/Entities/ (MODIFIED)
   ├── Complaint.cs : BaseAuditableEntity
   ├── TechnicalIntervention.cs : BaseAuditableEntity
   ├── Invoice.cs : BaseAuditableEntity
   ├── Client.cs : BaseAuditableEntity
   ├── Technician.cs : BaseAuditableEntity
   ├── Article.cs : BaseAuditableEntity
   └── Category.cs : BaseAuditableEntity

✅ SAV.Shared/Constants/
   └── AuthorizationConstants.cs (NEW)
	   ├── Roles
	   ├── Policies
	   ├── CustomClaimTypes
	   └── AuthorizationRequirements

✅ SAV.Shared/Interfaces/
   └── ICurrentUserService.cs (NEW)

✅ SAV.Infrastructure/Identity/
   ├── CurrentUserService.cs (NEW)
   ├── JwtTokenService.cs (MODIFIED - added custom claims)

✅ SAV.Infrastructure/Persistence/
   ├── SavDbContext.cs (MODIFIED - SaveChangesAsync override)
   └── Migrations/
	   └── AddAuditFieldsAndEnforceCleanArchitecture.cs (NEW)

✅ SAV.API/
   ├── Program.cs (MODIFIED - policies configuration)
   ├── Controllers/
	   ├── ComplaintsController.cs (MODIFIED - [Authorize])
	   └── InterventionsController.cs (MODIFIED - [Authorize])
```

### Étapes de déploiement

```bash
# 1. Appliquer la migration
cd SAV.Infrastructure
dotnet ef database update

# 2. Vérifier la compilation
cd SAV.API
dotnet build

# 3. Tester les endpoints
# - Avec Postman/Swagger
# - Vérifier les policies
# - Vérifier les audit fields
```

---

## Exemples d'utilisation

### Exemple 1: Créer une réclamation (Client)

```csharp
// Request
POST /api/complaints
Authorization: Bearer {client_token}
Content-Type: application/json

{
  "title": "Produit défectueux",
  "description": "Ne fonctionne pas",
  "clientId": 42,
  "articleId": 1
}

// Response 201 Created
{
  "id": 100,
  "title": "Produit défectueux",
  "description": "Ne fonctionne pas",
  "status": "Pending",
  "clientId": 42,
  "articleId": 1,
  "createdBy": 42,          // AUTO
  "createdDate": "2026-01-15T10:30:00Z",  // AUTO
  "updatedBy": null,
  "updatedDate": null,
  "isDeleted": false
}

// Database result
SELECT * FROM Complaints WHERE Id = 100;
/*
Id | Title | Description | Status | ClientId | ArticleId | CreatedBy | CreatedDate | UpdatedBy | UpdatedDate | IsDeleted | DeletedDate
100 | Produit défectueux | Ne fonctionne pas | Pending | 42 | 1 | 42 | 2026-01-15 10:30:00 | NULL | NULL | 0 | NULL
*/
```

### Exemple 2: Assigner un technicien (Admin)

```csharp
// Request
POST /api/interventions
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "type": "Repair",
  "scheduledDate": "2026-01-20",
  "complaintId": 100,
  "technicianId": 5
}

// Response 201 Created
{
  "id": 200,
  "type": "Repair",
  "status": "Scheduled",
  "scheduledDate": "2026-01-20",
  "complaintId": 100,
  "technicianId": 5,
  "createdBy": 1,          // Admin user ID
  "createdDate": "2026-01-15T11:00:00Z",
  "updatedBy": null,
  "updatedDate": null,
  "isDeleted": false
}
```

### Exemple 3: Client essayant d'accéder à autre's complaint

```csharp
// Request (Client 42 essayant d'accéder à client 99's complaint)
GET /api/complaints/200
Authorization: Bearer {client_42_token}

// Response 403 Forbidden
// Grâce au check dans le controller:
// if (!_currentUserService.IsAdmin() && result.ClientId != currentUserId)
//     return Forbid();
```

### Exemple 4: Soft delete (Admin)

```csharp
// Request
DELETE /api/complaints/100
Authorization: Bearer {admin_token}

// Response 204 No Content

// Database result (après soft delete)
SELECT * FROM Complaints WHERE Id = 100;
/*
Id | Title | ... | CreatedBy | CreatedDate | UpdatedBy | UpdatedDate | IsDeleted | DeletedDate
100 | ... | ... | 42 | 2026-01-15 10:30:00 | 1 | 2026-01-15 11:30:00 | 1 | 2026-01-15 11:30:00
*/

// Note: GetAll() ne retourne PAS cette entité grâce au query filter
GET /api/complaints
// La réclamation 100 n'est pas dans la liste
```

---

## Best Practices

### ✅ DO

```csharp
// 1. Toujours utiliser les constantes
[Authorize(Policy = Policies.AdminOnly)]
public async Task<ActionResult> Delete(int id) { }

// 2. Injector ICurrentUserService
public MyController(ICurrentUserService currentUserService)
{
	_currentUserService = currentUserService;
}

// 3. Utiliser les helpers pour vérifier les rôles
if (_currentUserService.IsAdmin()) { }
if (_currentUserService.HasAnyRole(Roles.Admin, Roles.Technician)) { }

// 4. Utiliser UTC dates
var now = DateTime.UtcNow;

// 5. Documenter les policies
[Authorize(Policy = Policies.AdminOrTechnician)]
public async Task<ActionResult> Update(int id, UpdateCommand cmd) { }

// 6. Vérifier l'ownership des ressources
if (result.ClientId != _currentUserService.GetCurrentUserId())
	return Forbid();
```

### ❌ DON'T

```csharp
// 1. ❌ Ne pas utiliser des magic strings
[Authorize(Policy = "AdminOnly")]  // ❌
[Authorize(Policy = Policies.AdminOnly)]  // ✅

// 2. ❌ Ne pas vérifier les roles manuellement
if (User.FindFirst(ClaimTypes.Role)?.Value == "Admin") { }  // ❌
if (_currentUserService.IsAdmin()) { }  // ✅

// 3. ❌ Ne pas manuellement modifier audit fields
entity.CreatedBy = userId;  // ❌ - Fait automatiquement par SaveChangesAsync

// 4. ❌ Ne pas utiliser DateTime.Now
var now = DateTime.Now;  // ❌ - Timezone issues
var now = DateTime.UtcNow;  // ✅

// 5. ❌ Ne pas oublier [Authorize]
[HttpGet]
public async Task<ActionResult> GetAll() { }  // ❌

[Authorize]
[HttpGet]
public async Task<ActionResult> GetAll() { }  // ✅
```

### 🎯 Authorization Checklist

Pour chaque endpoint, répondre:

- [ ] Quel rôle peut accéder?
- [ ] Faut-il vérifier l'ownership des ressources?
- [ ] Faut-il logger qui a fait quoi?
- [ ] Les audit fields sont-ils correctement populés?
- [ ] Y a-t-il une soft delete si nécessaire?
- [ ] Les indexes sont-ils en place pour les queries?

```csharp
// Template
[HttpPost]
[Authorize(Policy = Policies.???)]  // ← Quel policy?
public async Task<ActionResult> Create([FromBody] CreateCommand cmd)
{
	// Vérifier ownership si besoin
	var userId = _currentUserService.GetCurrentUserId();

	// Créer l'entité
	var entity = new MyEntity { };

	// SaveChangesAsync populate automatiquement audit fields
	await _context.SaveChangesAsync();

	return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
}
```

---

## Résumé

| Fonctionnalité | Implémentation | Automatique |
|---|---|---|
| **Role-Based Authorization** | Policy-based + JWT Claims | Non (à configurer) |
| **Current User Info** | ICurrentUserService | Oui (via IHttpContextAccessor) |
| **Audit Tracking** | BaseAuditableEntity + SaveChangesAsync | **Oui** |
| **Soft Delete** | IsDeleted + QueryFilter | **Oui** |
| **Audit Indexes** | OnModelCreating | **Oui** |
| **CreatedBy/Date** | SaveChangesAsync | **Oui** |
| **UpdatedBy/Date** | SaveChangesAsync | **Oui** |

---

**Version:** 1.0  
**Dernière mise à jour:** 15/01/2026  
**Auteur:** Backend Team
