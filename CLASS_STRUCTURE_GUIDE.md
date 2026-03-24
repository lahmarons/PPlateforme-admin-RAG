# IdaraTech Project Structure - Class Implementation Guide

## Overview
This document outlines the complete class structure created for your IdaraTech project following Clean Architecture and CQRS patterns with .NET Identity integration.

---

## 1. Domain Layer (src/Domain)

### Entities

#### 1.1 Utilisateur (Base Class)
**File:** `src/Domain/Entities/Utilisateur.cs`

Base abstract class for all user types, inheriting from `BaseAuditableEntity`:
- Properties:
  - `Nom` (string): User's full name
  - `Email` (string): Email address
  - `MotDePasse` (string): Password hash (encrypted)
- Audit Properties (inherited):
  - `Created`, `CreatedBy`, `LastModified`, `LastModifiedBy`

```
Inheritance Strategy: Table-Per-Type (TPT) with Discriminator
```

---

#### 1.2 Agent
**File:** `src/Domain/Entities/Agent.cs`

Inherits from `Utilisateur`. Represents government/organization agents:
- Additional Properties:
  - `Matricule` (string): Employee ID
  - `Grade` (string): Position/rank
  - `ServiceId` (int?): Foreign key to Service
  - `Service` (Navigation): Related Service entity
  - `DemandesToitees` (IList<Demande>): Requests handled by this agent
- Relationships:
  - One-to-Many with Demande (DemandesToitees)
  - Many-to-One with Service

---

#### 1.3 Citoyen
**File:** `src/Domain/Entities/Citoyen.cs`

Inherits from `Utilisateur`. Represents citizens:
- Additional Properties:
  - `Adresse` (string): Physical address
  - `Telephone` (string): Phone number
  - `Demandes` (IList<Demande>): Requests created by this citizen
- Relationships:
  - One-to-Many with Demande

---

#### 1.4 Admin
**File:** `src/Domain/Entities/Admin.cs`

Inherits from `Utilisateur`. Represents administrators:
- Properties:
  - `ServicesGeres` (IList<Service>): Services managed by admin
- Relationships:
  - Many-to-Many with Service (through AdminService junction table)

---

#### 1.5 Demande
**File:** `src/Domain/Entities/Demande.cs`

Represents citizen requests/complaints:
- Properties:
  - `Description` (string): Request details
  - `Statut` (Statut): Current status (see Enums)
  - `CitoyenId` (int): Foreign key to Citoyen
  - `ServiceId` (int?): Foreign key to Service
  - `AgentId` (int?): Foreign key to Agent handling the request
  - `Documents` (IList<Document>): Attached documents
- Methods:
  - `ChangerStatut(Statut)`: Update request status
- Relationships:
  - Many-to-One with Citoyen
  - Many-to-One with Service
  - Many-to-One with Agent
  - One-to-Many with Document

---

#### 1.6 Service
**File:** `src/Domain/Entities/Service.cs`

Represents government services:
- Properties:
  - `Nom` (string): Service name
  - `Description` (string): Service description
  - `Agents` (IList<Agent>): Agents working in this service
  - `Demandes` (IList<Demande>): Requests related to this service
- Relationships:
  - One-to-Many with Agent
  - One-to-Many with Demande

---

#### 1.7 Document
**File:** `src/Domain/Entities/Document.cs`

Represents documents attached to requests:
- Properties:
  - `Nom` (string): Document name/title
  - `Type` (string): File type (PDF, JPEG, etc.)
  - `DemandeId` (int): Foreign key to Demande
  - `Demande` (Navigation): Related Demande entity
- Relationships:
  - Many-to-One with Demande

---

### Enums

#### Statut
**File:** `src/Domain/Enums/Statut.cs`

Request status enumeration:
- `EnAttente` (0): Pending
- `EnCours` (1): In Progress
- `Traitee` (2): Processed/Approved
- `Refusee` (3): Refused/Denied

---

### Domain Events

Events that trigger during entity operations:

#### DemandeCreatedEvent
**File:** `src/Domain/Events/DemandeCreatedEvent.cs`
- Triggered when a new Demande is created
- Contains reference to the created Demande

#### DemandeStatusChangedEvent
**File:** `src/Domain/Events/DemandeStatusChangedEvent.cs`
- Triggered when Demande status changes
- Contains both new and previous status

#### UtilisateurCreatedEvent
**File:** `src/Domain/Events/UtilisateurCreatedEvent.cs`
- Triggered when a new Utilisateur (any type) is created
- Contains reference to the created Utilisateur

---

## 2. Application Layer (src/Application)

### Commands & Queries (CQRS Pattern)

#### 2.1 Create Demande Command
**Location:** `src/Application/Demandes/Commands/CreateDemande/`

- **Command:** `CreateDemandeCommand`
  - Properties: `CitoyenId`, `Description`, `ServiceId`
  - Response: `int` (newly created Demande ID)
  
- **Handler:** `CreateDemandeCommandHandler`
  - Creates new Demande entity
  - Publishes `DemandeCreatedEvent`
  - Persists to database
  
- **Validator:** `CreateDemandeCommandValidator`
  - Validates CitoyenId > 0
  - Validates Description not empty (max 1000 chars)

---

#### 2.2 Change Demande Status Command
**Location:** `src/Application/Demandes/Commands/ChangeDemandeStatus/`

- **Command:** `ChangeDemandeStatusCommand`
  - Properties: `DemandeId`, `NewStatus`
  - Response: Unit (void)
  
- **Handler:** `ChangeDemandeStatusCommandHandler`
  - Retrieves Demande by ID
  - Changes status using domain method
  - Publishes `DemandeStatusChangedEvent`
  - Persists changes

---

#### 2.3 Get Demande By ID Query
**Location:** `src/Application/Demandes/Queries/GetDemandById/`

- **Query:** `GetDemandeByIdQuery`
  - Properties: `Id`
  - Response: `DemandeDto`
  
- **Handler:** `GetDemandeByIdQueryHandler`
  - Retrieves Demande with all related data (includes)
  - Maps to DTO using AutoMapper
  - Returns structured data
  
- **DTO:** `DemandeDto`
  - Contains flattened data for API responses
  - Includes nested DocumentDto collection

---

## 3. Infrastructure Layer (src/Infrastructure)

### Database Context

#### ApplicationDbContext
**File:** `src/Infrastructure/Data/ApplicationDbContext.cs`

- Inherits from `IdentityDbContext<ApplicationUser>` (for .NET Identity)
- DbSets for all entities:
  - `Utilisateurs`, `Agents`, `Citoyens`, `Admins`
  - `Demandes`, `Services`, `Documents`
  - `TodoLists`, `TodoItems` (existing)

---

### Entity Configurations (Fluent API)

#### UtilisateurConfiguration
**File:** `src/Infrastructure/Data/Configurations/UtilisateurConfiguration.cs`
- Sets up TPT inheritance with discriminator
- Configures column constraints (max lengths)

#### AgentConfiguration
**File:** `src/Infrastructure/Data/Configurations/AgentConfiguration.cs`
- Configures relationships with Service and Demande
- Sets DeleteBehavior for foreign keys

#### CitoyenConfiguration
**File:** `src/Infrastructure/Data/Configurations/CitoyenConfiguration.cs`
- Configures one-to-many with Demande
- Cascade delete when Citoyen is removed

#### AdminConfiguration
**File:** `src/Infrastructure/Data/Configurations/AdminConfiguration.cs`
- Many-to-many relationship with Service
- Uses junction table pattern

#### DemandeConfiguration
**File:** `src/Infrastructure/Data/Configurations/DemandeConfiguration.cs`
- Configures all three foreign keys
- Status stored as int in database
- Cascade/SetNull delete behaviors

#### ServiceConfiguration
**File:** `src/Infrastructure/Data/Configurations/ServiceConfiguration.cs`
- Configures relationships with Agent and Demande
- SetNull delete behavior (services not deleted with agents)

#### DocumentConfiguration
**File:** `src/Infrastructure/Data/Configurations/DocumentConfiguration.cs`
- Configures relationship with Demande
- Cascade delete (documents deleted with request)

---

## 4. Updated Interfaces

### IApplicationDbContext
**File:** `src/Application/Common/Interfaces/IApplicationDbContext.cs`

Updated to include DbSet properties for all new entities:
```csharp
DbSet<Utilisateur> Utilisateurs { get; }
DbSet<Agent> Agents { get; }
DbSet<Citoyen> Citoyens { get; }
DbSet<Admin> Admins { get; }
DbSet<Demande> Demandes { get; }
DbSet<Service> Services { get; }
DbSet<Document> Documents { get; }
```

---

## 5. Architecture Patterns Used

### Clean Architecture Layers
- ✅ **Domain Layer**: Pure domain entities, no external dependencies
- ✅ **Application Layer**: CQRS Commands/Queries, business logic orchestration
- ✅ **Infrastructure Layer**: EF Core, database configuration, identity integration
- ✅ **Web Layer**: Razor Pages (user interface)

### CQRS Pattern
- **Commands**: CreateDemande, ChangeDemandeStatus (write operations)
- **Queries**: GetDemandeById (read operations)
- Separation of concerns ensures scalability

### Domain-Driven Design
- Domain events for important business occurrences
- Aggregate entities (Demande with Documents)
- Value Objects where appropriate

### Entity Framework
- Table-Per-Type (TPT) inheritance for Utilisateur hierarchy
- Fluent API configurations for complex relationships
- Proper cascade delete strategies

---

## 6. .NET Identity Integration

Your project uses `IdentityDbContext<ApplicationUser>`:
- ApplicationUser extends .NET Identity's IdentityUser
- Utilisateur is separate from Identity user (independent)
- Allows custom user profiles alongside Identity authentication

**Note:** You can link ApplicationUser to Utilisateur when needed using userId mapping

---

## 7. Next Steps

### Create Event Handlers
```
src/Application/Demandes/EventHandlers/DemandeCreatedEventHandler.cs
src/Application/Demandes/EventHandlers/DemandeStatusChangedEventHandler.cs
```

### Create Queries
```
src/Application/Demandes/Queries/GetDemandes/GetDemandes.cs (paginated list)
```

### Create Additional Commands
```
src/Application/Demandes/Commands/UpdateDemande/
src/Application/Demandes/Commands/DeleteDemande/
src/Application/Services/Commands/CreateService/
src/Application/Agents/Commands/AssignAgent/
```

### Configure AutoMapper Profiles
```
src/Application/Common/Mappings/DemandeMappingProfile.cs
src/Application/Common/Mappings/ServiceMappingProfile.cs
```

### Add Database Migration
```powershell
dotnet ef migrations add AddUtilisateurEntities --project src/Infrastructure --startup-project src/Web
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

---

## 8. Key Design Decisions

### Why BaseAuditableEntity?
- Automatically tracks Created/CreatedBy/LastModified/LastModifiedBy
- Auditing compliance for government applications
- Follows your existing project pattern

### Why Abstract Utilisateur?
- Cannot instantiate base user directly
- Forces creation of specific user types (Agent, Citoyen, Admin)
- Type safety and clearer domain intent

### Why IList<T> for Navigation Properties?
- Matches your existing pattern (see TodoItem/TodoList)
- Better with EF Core relationship loading
- Immutable collections for write operations

### Why Separate Demande and Document?
- Document can be reused across multiple demandes
- Clear responsibility separation
- Easier to manage file attachments

---

## 9. Database Schema Overview

```
Utilisateurs (TPT base table)
├── Agents (TPT child table with ServiceId FK)
├── Citoyens (TPT child table)
└── Admins (TPT child table)

Services
├── Foreign Keys from Agents (0..*)
└── Foreign Keys from Demandes (0..*)

Demandes
├── CitoyenId FK → Citoyens (1..1 required)
├── ServiceId FK → Services (0..1 optional)
├── AgentId FK → Agents (0..1 optional)
└── Documents (1..*)

Documents
└── DemandeId FK → Demandes (1..1 required)

AdminService (Junction Table)
├── AdminId FK → Admins
└── ServiceId FK → Services
```

---

## 10. Recommended File Structure Summary

```
src/
├── Domain/
│   ├── Common/
│   ├── Entities/
│   │   ├── Utilisateur.cs ✅
│   │   ├── Agent.cs ✅
│   │   ├── Citoyen.cs ✅
│   │   ├── Admin.cs ✅
│   │   ├── Demande.cs ✅
│   │   ├── Service.cs ✅
│   │   └── Document.cs ✅
│   ├── Enums/
│   │   └── Statut.cs ✅
│   └── Events/
│       ├── DemandeCreatedEvent.cs ✅
│       ├── DemandeStatusChangedEvent.cs ✅
│       └── UtilisateurCreatedEvent.cs ✅
├── Application/
│   ├── Demandes/
│   │   ├── Commands/
│   │   │   ├── CreateDemande/ ✅
│   │   │   └── ChangeDemandeStatus/ ✅
│   │   └── Queries/
│   │       └── GetDemandById/ ✅
│   └── Common/
│       └── Interfaces/
│           └── IApplicationDbContext.cs ✅
├── Infrastructure/
│   └── Data/
│       ├── ApplicationDbContext.cs ✅
│       └── Configurations/
│           ├── UtilisateurConfiguration.cs ✅
│           ├── AgentConfiguration.cs ✅
│           ├── CitoyenConfiguration.cs ✅
│           ├── AdminConfiguration.cs ✅
│           ├── DemandeConfiguration.cs ✅
│           ├── ServiceConfiguration.cs ✅
│           └── DocumentConfiguration.cs ✅
└── Web/
    └── Pages/
        └── (Razor Pages will follow CQRS structure)
```

---

## Summary

✅ **Completed:**
- 7 Domain Entities
- 1 Enumeration
- 3 Domain Events
- 3 CQRS Commands/Queries
- 2 Validators
- 1 DTO with nested DTO
- 7 EF Core Configurations
- Updated DbContext and Interface

**All following Clean Architecture and CQRS patterns with .NET 10 and .NET Identity integration.**
