# IdaraTech - Quick Start Guide

## What Has Been Created

Your project now has a complete domain model for an e-government application with:
- ✅ 7 Domain Entities (User hierarchy, Requests, Services, Documents)
- ✅ 1 Enumeration (Request Status)
- ✅ 3 Domain Events
- ✅ Basic CQRS Commands & Queries for requests (Demandes)
- ✅ EF Core configurations for all entities
- ✅ Proper relationships and cascade behaviors

---

## Step 1: Create Database Migration

Run this command in your terminal:

```powershell
cd C:\Dev\IdaraTech\
dotnet ef migrations add AddUtilisateurEntities --project src/Infrastructure --startup-project src/Web
```

Expected output:
```
Build started...
Build succeeded.
To undo this action, use 'dotnet ef migrations down'.
```

---

## Step 2: Apply Migration to Database

```powershell
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

Expected output:
```
Build started...
Build succeeded.
Applying migration '20240101000000_AddUtilisateurEntities'.
Done.
```

---

## Step 3: Verify Classes in Solution Explorer

Open Visual Studio and check:

### Domain Layer
- `src/Domain/Entities/`
  - ✅ Utilisateur.cs (abstract base class)
  - ✅ Agent.cs
  - ✅ Citoyen.cs
  - ✅ Admin.cs
  - ✅ Demande.cs
  - ✅ Service.cs
  - ✅ Document.cs

- `src/Domain/Enums/`
  - ✅ Statut.cs

- `src/Domain/Events/`
  - ✅ DemandeCreatedEvent.cs
  - ✅ DemandeStatusChangedEvent.cs
  - ✅ UtilisateurCreatedEvent.cs

### Application Layer
- `src/Application/Demandes/Commands/`
  - ✅ CreateDemande/CreateDemande.cs
  - ✅ CreateDemande/CreateDemandeCommandValidator.cs
  - ✅ ChangeDemandeStatus/ChangeDemandeStatus.cs

- `src/Application/Demandes/Queries/`
  - ✅ GetDemandById/GetDemandeById.cs
  - ✅ GetDemandById/DemandeDto.cs

### Infrastructure Layer
- `src/Infrastructure/Data/`
  - ✅ ApplicationDbContext.cs (updated)
  - ✅ Configurations/ (6 entity configurations)

---

## Step 4: Build Solution

```powershell
dotnet build
```

All projects should build successfully (ignoring the AutoMapper vulnerability warning).

---

## Next: Implement Remaining Commands

Your project skeleton is ready. Here are the next steps you should implement:

### 1. Create User Management Commands

```
src/Application/Utilisateurs/Commands/CreateCitoyen/
src/Application/Utilisateurs/Commands/CreateAgent/
src/Application/Utilisateurs/Commands/UpdateUtilisateur/
src/Application/Utilisateurs/Commands/DeleteUtilisateur/
```

### 2. Create Service Management Commands

```
src/Application/Services/Commands/CreateService/
src/Application/Services/Commands/UpdateService/
src/Application/Services/Queries/GetServices/
```

### 3. Create Event Handlers

```
src/Application/Demandes/EventHandlers/DemandeCreatedEventHandler.cs
src/Application/Demandes/EventHandlers/DemandeStatusChangedEventHandler.cs
src/Application/Utilisateurs/EventHandlers/UtilisateurCreatedEventHandler.cs
```

### 4. Update Your Razor Pages

Connect your Razor Pages to the CQRS commands:

```csharp
// Pages/Demandes/Create.cshtml.cs

public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    
    public CreateModel(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [BindProperty]
    public CreateDemandeCommand Input { get; set; }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _mediator.Send(Input);
        return RedirectToPage("./Index");
    }
}
```

---

## Class Relationships Summary

```
┌─────────────────────────────────────────────┐
│           Utilisateur (Abstract)            │
│    id, nom, email, motDePasse, dates      │
└──────────────┬──────────────┬───────────────┘
               │              │
         ┌─────▼──┐      ┌────▼─────┐
         │ Agent  │      │  Citoyen │
         └────┬───┘      └────┬─────┘
              │                │
              │         ┌──────▼────────┐
              │         │               │
         ┌────▼─────┐  ┌▼────────┐   ┌──▼──────┐
         │ Service  │  │ Demande │   │Document │
         └──────────┘  └─────────┘   └─────────┘
         (1 to many)      (many)        (many)
              │             │             │
              └─────┬───────┴─────────────┘
                    │
              Many-to-One/Many
```

---

## Database Relationships (Entity Framework)

### Inheritance Strategy
- **Table-Per-Type (TPT)**: Utilisateur gets its own table
  - Agents table has FK to Utilisateurs
  - Citoyens table has FK to Utilisateurs
  - Admins table has FK to Utilisateurs

### Foreign Keys
- `Demande.CitoyenId` → `Citoyens.Id` (required, cascade delete)
- `Demande.ServiceId` → `Services.Id` (optional, set null)
- `Demande.AgentId` → `Agents.Id` (optional, set null)
- `Agent.ServiceId` → `Services.Id` (optional, set null)
- `Document.DemandeId` → `Demandes.Id` (required, cascade delete)

---

## Testing Your Setup

### Create a Citoyen (in a Razor Page)

```csharp
var citoyen = new Citoyen
{
    Nom = "John Doe",
    Email = "john@example.com",
    MotDePasse = "hashed_password", // Use proper password hashing
    Adresse = "123 Main St",
    Telephone = "555-1234"
};

// Add to context
context.Citoyens.Add(citoyen);
await context.SaveChangesAsync();
```

### Create a Demande (using CQRS)

```csharp
var command = new CreateDemandeCommand
{
    CitoyenId = 1,
    Description = "I need a birth certificate",
    ServiceId = null // Optional
};

var demandeId = await _mediator.Send(command);
```

### Change Status (using CQRS)

```csharp
var command = new ChangeDemandeStatusCommand
{
    DemandeId = 1,
    NewStatus = Statut.EnCours
};

await _mediator.Send(command);
```

### Get Demande Details (using CQRS)

```csharp
var query = new GetDemandeByIdQuery { Id = 1 };
var demandeDto = await _mediator.Send(query);
```

---

## Key Files Created

| File | Purpose |
|------|---------|
| `src/Domain/Entities/*.cs` | Domain model entities |
| `src/Domain/Enums/Statut.cs` | Request status enumeration |
| `src/Domain/Events/*.cs` | Domain events for business operations |
| `src/Application/Demandes/**` | CQRS commands & queries |
| `src/Infrastructure/Data/Configurations/*.cs` | EF Core entity configurations |
| `CLASS_STRUCTURE_GUIDE.md` | Complete architectural documentation |

---

## Important Notes

### Clean Architecture Layers
1. **Domain**: Pure business logic, no dependencies
2. **Application**: Use cases, CQRS, business orchestration
3. **Infrastructure**: EF Core, database, external services
4. **Web**: Razor Pages, user interface

### CQRS Pattern Benefits
- ✅ Separation of read and write operations
- ✅ Easy to scale query side independently
- ✅ Clear command intent
- ✅ Better testability

### .NET Identity Integration
- Your `Utilisateur` entities are separate from ASP.NET Identity `ApplicationUser`
- Use Identity for authentication (login)
- Use Utilisateur for application business logic
- Link them via userId when needed

---

## Troubleshooting

### Migration Issues
```powershell
# Remove last migration
dotnet ef migrations remove --project src/Infrastructure

# Recreate migration
dotnet ef migrations add AddUtilisateurEntities --project src/Infrastructure

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

### Build Errors
- Check that all using statements are present
- Verify namespace matches folder structure
- Clean and rebuild solution: `dotnet clean && dotnet build`

---

## Reference

For more details on the class structure, see: **CLASS_STRUCTURE_GUIDE.md**

Happy coding! 🚀
