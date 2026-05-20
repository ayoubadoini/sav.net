# 🗂️ Visual Reference Guide

## Enterprise Features at a Glance

### 🔐 Authorization System

```
┌─────────────────────────────────────────────────┐
│          JWT TOKEN STRUCTURE                     │
├─────────────────────────────────────────────────┤
│  Header: {                                      │
│    "alg": "HS256",                             │
│    "typ": "JWT"                                │
│  }                                             │
│                                                 │
│  Payload: {                                     │
│    "sub": "42",                   ← User ID     │
│    "email": "client@example.com",              │
│    "name": "John Client",                      │
│    "role": "Client",              ← Role       │
│    "UserId": "42",                             │
│    "FullName": "John Client",                  │
│    "IsActive": true,                           │
│    "iat": 1705338600,             ← Issued At  │
│    "exp": 1705338900              ← Expiry     │
│  }                                             │
│                                                 │
│  Signature: HMACSHA256(...)       ← Signed     │
└─────────────────────────────────────────────────┘
```

### 🎭 Roles & Policies Matrix

```
┌──────────────┬──────────┬───────────┬─────────┐
│  Endpoint    │  Admin   │ Technician│ Client  │
├──────────────┼──────────┼───────────┼─────────┤
│ GET /api/... │    ✅    │     ✅    │   ✅    │
│ POST /api/   │  AdminOn │  TechOn   │ClientOn │
│ PUT /api/    │ AdminOrTe│ AdminOrTe │   ❌    │
│ DELETE /api/ │AdminOnly │   ❌      │   ❌    │
└──────────────┴──────────┴───────────┴─────────┘

Legend:
✅ = Allowed for all authenticated users
❌ = Forbidden for this role
AdminOnly = Only Admin
TechnicianOnly = Only Technician
ClientOnly = Only Client
AdminOrTechnician = Admin OR Technician
```

### 📊 Audit Fields Timeline

```
CREATED (t=0)
├─ CreatedBy = 42 (User ID who created)
├─ CreatedDate = 2026-01-15 10:00:00Z
├─ UpdatedBy = null
├─ UpdatedDate = null
├─ IsDeleted = false
└─ DeletedDate = null

UPDATED (t=1 hour)
├─ CreatedBy = 42 (PROTECTED - unchanged)
├─ CreatedDate = 2026-01-15 10:00:00Z (PROTECTED - unchanged)
├─ UpdatedBy = 1 (User ID who updated)
├─ UpdatedDate = 2026-01-15 11:00:00Z
├─ IsDeleted = false
└─ DeletedDate = null

DELETED (t=2 hours)
├─ CreatedBy = 42 (PROTECTED - unchanged)
├─ CreatedDate = 2026-01-15 10:00:00Z (PROTECTED - unchanged)
├─ UpdatedBy = 1 (User ID who deleted - soft delete)
├─ UpdatedDate = 2026-01-15 12:00:00Z
├─ IsDeleted = true (SOFT DELETE - not hard delete)
└─ DeletedDate = 2026-01-15 12:00:00Z
```

### 🔄 SaveChangesAsync Flow

```
┌──────────────────────┐
│  SaveChangesAsync()  │ ← Entity change detected
└──────────┬───────────┘
		   ▼
┌──────────────────────────────────┐
│  ApplyAuditingInfo()             │
└──────────┬───────────────────────┘
		   ▼
	 ┌─────┴─────┐
	 │           │
	 ▼           ▼
 EntityState  EntityState     ...
 .Added       .Modified
	 │           │
	 ▼           ▼
Set CreatedBy  Don't modify CreatedBy/Date (Protected)
Set CreatedDate Set UpdatedBy = current user
		 Set UpdatedDate = now

	 ├─ EntityState.Deleted ─┐
	 │                         │
	 └─────────────┬───────────┘
				   ▼
		 Change to EntityState.Modified
		 Set IsDeleted = true
		 Set DeletedDate = now
		 Set UpdatedBy = current user
		 Set UpdatedDate = now
				   │
				   ▼
		┌──────────────────────┐
		│ base.SaveChangesAsync│ ← Persist to DB
		└─────────────────────┘
```

### 🗄️ Database Schema (Audit Fields)

```
Complaints Table (Example)
┌─────────────────────────────────────────────┐
│ Column              │ Type        │ Nullable │
├─────────────────────────────────────────────┤
│ Id                  │ int         │ No       │
│ Title               │ varchar(255)│ No       │
│ Description         │ text        │ No       │
│ Status              │ int         │ No       │
│ ClientId            │ int         │ No       │
│ ArticleId           │ int         │ No       │
├─ AUDIT FIELDS ──────────────────────────────┤
│ CreatedBy           │ int         │ Yes      │ ← User ID
│ CreatedDate         │ datetime2   │ No       │ ← UTC
│ UpdatedBy           │ int         │ Yes      │ ← User ID
│ UpdatedDate         │ datetime2   │ Yes      │ ← UTC
│ IsDeleted           │ bit         │ No       │ ← Soft delete
│ DeletedDate         │ datetime2   │ Yes      │ ← UTC
└─────────────────────────────────────────────┘

Indexes:
┌──────────────────────┐
│ IX_Complaints_IsDeleted ← For soft delete queries
│ IX_Complaints_CreatedBy ← For audit trail queries
└──────────────────────┘
```

### 🎯 Request Authorization Flow

```
CLIENT REQUEST
	 │
	 ├─ GET /api/complaints
	 │  Authorization: Bearer {token}
	 │
	 ▼
┌────────────────────────────┐
│ JWT Middleware             │
│ - Validate signature       │
│ - Check expiration        │
│ - Extract claims          │
└────────────┬───────────────┘
			 ▼
		┌─────────────────┐
		│ Claims Principal│
		│ sub: 42         │
		│ role: Client    │
		│ email: ...      │
		└────────┬────────┘
				 ▼
	  ┌──────────────────────┐
	  │ [Authorize] Check    │
	  │ - Is authenticated?  │
	  └──────────┬───────────┘
				 │ ✅ Yes
				 ▼
	  ┌──────────────────────┐
	  │ Policy Check         │
	  │ [Authorize(Policy)]  │
	  │ - Role required?     │
	  └──────────┬───────────┘
				 │ ✅ ClientOnly
				 ▼
	  ┌──────────────────────┐
	  │ Execute Controller   │
	  │ - ICurrentUserService│
	  │ - GetCurrentUserId() │
	  │ - Ownership checks   │
	  └──────────┬───────────┘
				 ▼
	  ┌──────────────────────┐
	  │ Business Logic       │
	  │ - Repository access  │
	  │ - DbContext changes  │
	  └──────────┬───────────┘
				 ▼
	  ┌──────────────────────┐
	  │ SaveChangesAsync()   │
	  │ - ApplyAuditingInfo()│
	  │ - Populate audit     │
	  │ - Persist to DB      │
	  └──────────┬───────────┘
				 ▼
	  ┌──────────────────────┐
	  │ Return Response      │
	  │ 200/201/204/403/401  │
	  └──────────────────────┘
```

### 📈 Role Hierarchy

```
					┌─────────┐
					│  Admin  │  (System Admin)
					│ Roles:1 │
					└────┬────┘
						 │
		 ┌───────────────┼────────────────┐
		 │               │                │
		 ▼               ▼                ▼
	┌─────────┐    ┌──────────┐    ┌──────────┐
	│  Admin  │    │Technician│    │  Client  │
	│ (Full)  │    │ (Service)│    │(Limited) │
	│ Roles:1 │    │ Roles:2  │    │ Roles:3  │
	└────┬────┘    └────┬─────┘    └────┬─────┘
		 │              │              │
	Can:             Can:            Can:
	- Manage all     - Interventions - View own
	- Delete/Update  - Assign tasks  - Create
	- Create users   - Update status - Update own
	- View reports   - View assigned - Request
	- Delete all     - Track time    - Pay
	- Full access    - Limited scope - Limited
```

### 🔗 Entity Relationships with Audit

```
User (AspNetUser)
  ├─ Id ...................... int
  └─ RoleId .................. int
	   │
	   ▼
	 Role ..................... Complaints ◄────┐
	   │                            │            │
	   │                    ┌────────┤────────┐  │
	   │                    │   ▼    ▼        ▼  │
	   │              CreatedBy UpdatedBy  IsDeleted (audit)
	   │                    │   ▼        ▼
	   │                 User  DateTime  bool
	   │                    │
	   ▼
	Client ..................... Articles
	   │                            │
	   │                    ┌───────┤────────┐
	   │                    │       ▼        ▼
	   │            CreatedBy  CreatedDate  IsDeleted
	   │                    │       │
	   └──────┬─────────────┘  DateTime
			  │
		 Complaints ........... TechnicalIntervention
			  │                       │
			  ├─ Client ◄──────────┬──┤
			  │                    │
			  └─ Article   ┌───────┤────────┐
						  │   ▼    ▼        ▼
					  CreatedBy UpdatedBy  IsDeleted

```

### ⏱️ Timestamp Reference

```
Request at 2026-01-15 14:30:45 EST (UTC-5)

Backend receives:
   DateTime.UtcNow = 2026-01-15 19:30:45 UTC ← Always UTC

Database stores:
   CreatedDate = 2026-01-15 19:30:45.000000

Why UTC?
   ✅ Timezone-independent
   ✅ Consistent across servers
   ✅ Audit trail always accurate
   ✅ International compliance
```

### 🛡️ Security Layers

```
LAYER 1: Transport
┌─────────────────────────────────┐
│ HTTPS / TLS 1.2+                │
│ Encrypted in transit            │
└─────────────────────────────────┘
		 │ Token
		 ▼
LAYER 2: Authentication
┌─────────────────────────────────┐
│ JWT Token                       │
│ Signed with secret key          │
│ Expiration: 15 minutes          │
└─────────────────────────────────┘
		 │ Verified
		 ▼
LAYER 3: Authorization (1st)
┌─────────────────────────────────┐
│ [Authorize] Attribute           │
│ User authenticated?             │
└─────────────────────────────────┘
		 │ ✅ Yes
		 ▼
LAYER 4: Authorization (2nd)
┌─────────────────────────────────┐
│ [Authorize(Policy = "...")] │
│ User has required role?         │
└─────────────────────────────────┘
		 │ ✅ Yes
		 ▼
LAYER 5: Business Logic
┌─────────────────────────────────┐
│ Ownership verification          │
│ Can user access this resource?  │
└─────────────────────────────────┘
		 │ ✅ Yes
		 ▼
LAYER 6: Data Persistence
┌─────────────────────────────────┐
│ Audit tracking                  │
│ Who, When, What, Why logged     │
└─────────────────────────────────┘
		 │
		 ▼
LAYER 7: Database
┌─────────────────────────────────┐
│ SQL Server                      │
│ Connection string encrypted     │
│ Data encrypted at rest (optional)
└─────────────────────────────────┘
```

---

## 📋 Quick Reference Cards

### Authorization Constants

```csharp
// ROLES
Roles.Admin = "Admin"
Roles.Technician = "Technician"
Roles.Client = "Client"

// POLICIES
Policies.AdminOnly = "AdminOnly"
Policies.TechnicianOnly = "TechnicianOnly"
Policies.ClientOnly = "ClientOnly"
Policies.AdminOrTechnician = "AdminOrTechnician"
Policies.AdminOrClient = "AdminOrClient"
Policies.Authenticated = "Authenticated"

// CLAIM TYPES
CustomClaimTypes.Role = "role"
CustomClaimTypes.UserId = "UserId"
CustomClaimTypes.Email = "email"
CustomClaimTypes.FullName = "FullName"
CustomClaimTypes.IsActive = "IsActive"
```

### Audit Field Methods

```csharp
// Check what user ID made the change
var auditLog = await _context.Complaints
	.Where(c => c.CreatedBy == userId)
	.ToListAsync();

// Find what was updated when
var recentUpdates = await _context.Complaints
	.Where(c => c.UpdatedDate >= DateTime.UtcNow.AddHours(-1))
	.ToListAsync();

// Find deleted records (for recovery)
var deletedItems = await _context.Complaints
	.IgnoreQueryFilters()
	.Where(c => c.IsDeleted)
	.ToListAsync();

// Track who deleted what
var deletionAudit = await _context.Complaints
	.IgnoreQueryFilters()
	.Where(c => c.IsDeleted && c.DeletedDate.HasValue)
	.Select(c => new {
		c.Id,
		c.Title,
		DeletedBy = c.UpdatedBy,
		DeletedAt = c.DeletedDate
	})
	.ToListAsync();
```

---

**This visual guide provides a quick reference for understanding the enterprise features architecture.**
