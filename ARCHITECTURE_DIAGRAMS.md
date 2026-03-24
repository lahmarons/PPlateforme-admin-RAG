# IdaraTech Architecture Diagrams

## Complete Class Hierarchy

```
┌─────────────────────────────────────────────────────────────┐
│                    BaseEntity                               │
│              (ID + Domain Events)                           │
└────────────────────────┬────────────────────────────────────┘
                         │
        ┌────────────────┴────────────────┐
        │                                 │
        ▼                                 ▼
   ┌─────────────┐            ┌──────────────────────┐
   │ BaseEntity  │            │ BaseAuditableEntity  │
   │  (Default)  │            │  (Created/Modified)  │
   └─────────────┘            └──────────┬───────────┘
                                        │
                    ┌───────────────────┼───────────────────┐
                    │                   │                   │
                    ▼                   ▼                   ▼
               ┌──────────┐        ┌──────────┐       ┌──────────┐
               │ Demande  │        │ Service  │       │Utilisateur│
               │  (TPT)   │        │  (TPT)   │       │(TPT-Base) │
               │ id       │        │ id       │       │ id       │
               │ desc     │        │ nom      │       │ nom      │
               │ statut   │        │ desc     │       │ email    │
               └──────────┘        └──────────┘       │ pwd      │
                    │                   │             └────┬─────┘
                    │                   │                  │
                ┌───┴────────┐      ┌───┴────┐        ┌────┴────┬─────────┬──────┐
                │            │      │        │        │         │         │      │
                ▼            ▼      ▼        ▼        ▼         ▼         ▼      ▼
            ┌────────┐  ┌────────┐┌───────┐┌───────┐┌────────┬─────────┬─────────┬────────┐
            │Document│  │  FK    ││Agents ││Demande││ Agent  │ Citoyen │  Admin  │ Direct │
            │(Demande│  │        ││(1..*)││(1..*)││(Agent)│(User)  │(Admin)  │Inherit │
            │attach) │  │        ││      ││      ││       │        │        │       │
            │ id     │  │Service ││      ││      ││matricule
           │ nom    │  │  id    ││      ││      ││grade   │Adresse │Services │       │
            │ type   │  │        ││      ││      ││svcId   │Telephone
           │        │  │        ││      ││      ││        │        │        │       │
            └────────┘  └────────┘└───────┘└───────┘└────────┴─────────┴─────────┴────────┘
```

---

## CQRS Processing Pipeline

```
┌──────────────────────────────────────────────────────────────────┐
│                          User Request                            │
│                  (Razor Page / Button Click)                     │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                    ┌────────▼────────┐
                    │   Send Command  │
                    │  or Send Query  │
                    └────────┬────────┘
                             │
        ┌────────────────────┴────────────────────┐
        │                                         │
        ▼                                         ▼
  ┌──────────────┐                        ┌──────────────┐
  │   COMMAND    │                        │    QUERY     │
  │  (Write Op)  │                        │  (Read Op)   │
  │              │                        │              │
  │ Create       │                        │ GetById      │
  │ Update       │                        │ GetList      │
  │ Delete       │                        │ Search       │
  │ ChangeStatus │                        │              │
  └──────┬───────┘                        └──────┬───────┘
         │                                       │
         ▼                                       ▼
  ┌──────────────────┐                   ┌──────────────────┐
  │ ValidationBhvr   │                   │ QueryHandler     │
  │ LoggingBhvr      │                   │                  │
  │ PerfBhvr         │                   │ .Include()       │
  │ AuthorizationBhvr│                   │ .Map to DTO      │
  └──────┬───────────┘                   │ return result    │
         │                               └──────┬───────────┘
    ✅/❌ Valid?                                  │
         │                                       │
  ┌──────▼──────────────┐                       │
  │ Command Handler     │                       │
  │                     │                       │
  │ Create Entity       │                       │
  │ Apply Logic         │                       │
  │ AddDomainEvent      │                       │
  │ SaveChanges         │                       │
  └──────┬──────────────┘                       │
         │                                       │
  ┌──────▼──────────────────────────────────────▼────────┐
  │           Return Result / DTO                        │
  │    (To Page Handler / API Controller)               │
  └───────────────────────────────────────────────────────┘
         │
    ┌────▼───────┐
    │  Display   │
    │   Result   │
    └────────────┘
```

---

## Entity Relationship Diagram (ERD)

```
                    ┌────────────────┐
                    │  UTILISATEURS  │
                    │  (TPT Base)    │
                    │                │
                    │ id (PK)        │
                    │ nom            │
                    │ email          │
                    │ motDePasse     │
                    │ Created        │
                    │ CreatedBy      │
                    │ LastModified   │
                    │ LastModifiedBy │
                    └────────────────┘
                           △
                           │ (TPT Inheritance)
          ┌────────────────┼────────────────┐
          │                │                │
          ▼                ▼                ▼
    ┌─────────┐      ┌──────────┐     ┌────────┐
    │ AGENTS  │      │ CITOYENS │     │ ADMINS │
    │         │      │          │     │        │
    │ id (FK) │      │ id (FK)  │     │id (FK) │
    │ matricule
    │ grade   │      │ adresse  │     │        │
    │ ServiceId─┐    │ telephone│     │        │
    └─────┬───┘│    └──────┬───┘     └──┬──────┘
          │    │           │            │
          │    │    ┌──────▼────┐       │
          │    │    │ DEMANDES  │       │
          │    │    │           │       │
          │    │    │ id (PK)   │       │
          │    │    │ description
          │    │    │ statut    │       │
          │    │    │ CitoyenId─┼──────►(Citoyen)
          │    │    │ ServiceId─┼──┐    │
          │    │    │ AgentId ──┼──┼──┐ │
          │    │    │ Created   │  │  │ │
          │    │    │ LastMod   │  │  │ │
          │    │    └──────┬────┘  │  │ │
          │    │           │       │  │ │
          │    └─────────────────┐ │  │ │
          │                      │ │  │ │
          │    ┌─────────────┐  │ │  │ │
          │    │ SERVICES    │  │ │  │ │
          │    │             │  │ │  │ │
          │    │ id (PK)     │  │ │  │ │
          │    │ nom         │  │ │  │ │
          │    │ description │  │ │  │ │
          │    │ Created     │  │ │  │ │
          │    │ LastMod     │  │ │  │ │
          │    └─────┬───────┘  │ │  │ │
          │          │          │ │  │ │
          │          └──────┐   │ │  │ │
          │                 │   │ │  │ │
          └─────────────────┼───┘ │  │ │
                            │     │  │ │
                            │     │  │ │
                    ┌───────▼──────┘  │ │
                    │                 │ │
                    ▼                 ▼ ▼
              ┌──────────────────────────────┐
              │ ADMINSERVICE (Junction)      │
              │                              │
              │ AdminId (FK→ADMINS)          │
              │ ServiceId (FK→SERVICES)      │
              └──────────────────────────────┘

┌──────────────────┐
│   DOCUMENTS      │
│                  │
│ id (PK)          │
│ nom              │
│ type             │
│ DemandeId (FK)◄──┤
│                  │
│ Created          │
│ LastModified     │
└──────────────────┘
```

---

## Request Lifecycle (State Machine)

```
┌─────────────┐
│ EnAttente   │  ◄──── New request created
│ (Pending)   │       by Citoyen
└──────┬──────┘
       │ Service receives request
       │ (Admin assigns to Agent)
       ▼
┌──────────────┐
│ EnCours      │  ◄──── Agent processes
│ (In Progress)│       the request
└──────┬───────┘
       │ Agent completes action
       │
       ├─────────────────┬──────────────────┐
       │                 │                  │
       ▼                 ▼                  ▼
  ┌───────────┐   ┌─────────────┐    ┌──────────┐
  │ Traitee   │   │ Refusee     │    │(Optional)│
  │ (Processed│   │ (Rejected)  │    │Archivé  │
  │ /Approved)│   │             │    │         │
  └───────────┘   └─────────────┘    └──────────┘

Events Published at Each Transition:
├─ DemandeCreatedEvent (at creation)
├─ DemandeStatusChangedEvent (at each status change)
└─ Custom handlers can react to events
```

---

## Application Layers (Dependency Flow)

```
┌─────────────────────────────────────────────────────────┐
│         WEB LAYER (Presentation)                        │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Razor Pages / Page Models                        │  │
│  │ ├─ @page directive                               │  │
│  │ ├─ OnGetAsync() / OnPostAsync()                   │  │
│  │ └─ Inject IMediator                              │  │
│  └──────────────────┬───────────────────────────────┘  │
└─────────────────────┼────────────────────────────────────┘
                      │ depends on
                      ▼
┌─────────────────────────────────────────────────────────┐
│      APPLICATION LAYER (Business Logic)                │
│  ┌──────────────────────────────────────────────────┐  │
│  │ CQRS Commands & Queries                          │  │
│  │ ├─ Commands                                       │  │
│  │ │  ├─ CreateDemandeCommand                        │  │
│  │ │  ├─ ChangeDemandeStatusCommand                  │  │
│  │ │  └─ [Add more as needed]                        │  │
│  │ ├─ Queries                                        │  │
│  │ │  ├─ GetDemandeByIdQuery                         │  │
│  │ │  └─ [Add more as needed]                        │  │
│  │ ├─ DTOs                                           │  │
│  │ │  ├─ DemandeDto                                  │  │
│  │ │  └─ DocumentDto                                 │  │
│  │ ├─ Validators                                     │  │
│  │ │  └─ CreateDemandeCommandValidator               │  │
│  │ ├─ Event Handlers                                 │  │
│  │ │  └─ [To be created]                             │  │
│  │ └─ Mappings (AutoMapper)                          │  │
│  │    └─ DemandeMappingProfile                       │  │
│  └──────────────────┬───────────────────────────────┘  │
└─────────────────────┼────────────────────────────────────┘
                      │ depends on
                      ▼
┌─────────────────────────────────────────────────────────┐
│        DOMAIN LAYER (Pure Business Rules)              │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Entities                                         │  │
│  │ ├─ Utilisateur (abstract base)                   │  │
│  │ ├─ Agent                                         │  │
│  │ ├─ Citoyen                                       │  │
│  │ ├─ Admin                                         │  │
│  │ ├─ Demande                                       │  │
│  │ ├─ Service                                       │  │
│  │ └─ Document                                      │  │
│  │ Enums                                            │  │
│  │ └─ Statut (EnAttente, EnCours, Traitee, Refusee)│  │
│  │ Domain Events                                    │  │
│  │ ├─ DemandeCreatedEvent                           │  │
│  │ ├─ DemandeStatusChangedEvent                     │  │
│  │ └─ UtilisateurCreatedEvent                       │  │
│  └──────────────────┬───────────────────────────────┘  │
└─────────────────────┼────────────────────────────────────┘
                      │ depends on
                      ▼
┌─────────────────────────────────────────────────────────┐
│     INFRASTRUCTURE LAYER (Technical Details)           │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Database & ORM (Entity Framework)                │  │
│  │ ├─ ApplicationDbContext                          │  │
│  │ ├─ Entity Configurations (Fluent API)            │  │
│  │ │  ├─ UtilisateurConfiguration                   │  │
│  │ │  ├─ AgentConfiguration                         │  │
│  │ │  ├─ CitoyenConfiguration                       │  │
│  │ │  ├─ AdminConfiguration                         │  │
│  │ │  ├─ DemandeConfiguration                       │  │
│  │ │  ├─ ServiceConfiguration                       │  │
│  │ │  └─ DocumentConfiguration                      │  │
│  │ └─ Migrations                                    │  │
│  │    └─ [To be created]                            │  │
│  │                                                   │  │
│  │ External Services (when needed)                  │  │
│  │ ├─ Email Service                                 │  │
│  │ ├─ Storage Service                               │  │
│  │ └─ Identity Service                              │  │
│  └──────────────────┬───────────────────────────────┘  │
└─────────────────────┼────────────────────────────────────┘
                      │
                      ▼
              ┌──────────────────┐
              │   DATABASE       │
              │   SQL Server     │
              │   (Production)   │
              └──────────────────┘
```

---

## Data Flow Example: Create Request

```
1. User fills form and clicks Submit
   └─────────────────┬────────────────┐
                     │                │
   ┌─────────────────▼────────┐       │
   │ Razor Page Handler       │       │
   │ OnPostAsync()            │       │
   │                          │       │
   │ Input validation         │       │
   │ Create command           │       │
   │ Send via MediatR         │       │
   └─────────────────┬────────┘       │
                     │                │
   ┌─────────────────▼─────────────────────────┐
   │ MediatR Pipeline                          │
   │ 1. ValidationBehaviour ──► Validate input │
   │ 2. AuthorizationBehaviour               │
   │ 3. LoggingBehaviour                     │
   │ 4. PerformanceBehaviour                 │
   └─────────────────┬──────────────────────────┘
                     │
       ┌─────────────▼──────────────┐
       │ CreateDemandeCommandHandler │
       │ .Handle()                   │
       │                             │
       │ 1. Create Demande entity    │
       │ 2. Add domain event         │
       │ 3. SaveChanges()            │
       │ 4. Return new ID            │
       └─────────────────┬───────────┘
                         │
       ┌─────────────────▼──────────────────┐
       │ Domain Events Published            │
       │                                    │
       │ DemandeCreatedEvent handlers:      │
       │ ├─ Send email notification         │
       │ ├─ Create audit log                │
       │ ├─ Update statistics               │
       │ └─ Trigger workflows               │
       └─────────────────┬──────────────────┘
                         │
       ┌─────────────────▼──────────────────┐
       │ Database Updated                   │
       │                                    │
       │ INSERT INTO Demandes               │
       │   (description, statut, CitoyenId) │
       │ VALUES (...)                       │
       └─────────────────┬──────────────────┘
                         │
       ┌─────────────────▼──────────────────┐
       │ Return Result to Page Handler      │
       │                                    │
       │ demandeId = 42                     │
       └─────────────────┬──────────────────┘
                         │
       ┌─────────────────▼──────────────────┐
       │ Display Success & Redirect         │
       │                                    │
       │ "Request created successfully!"    │
       │ Redirect to Details page           │
       └────────────────────────────────────┘
```

---

## Validation & Error Handling Flow

```
User Input
    │
    ▼
┌──────────────────────────────┐
│ Command/Query Created        │
│ (with user input values)     │
└──────────────┬───────────────┘
               │
    ┌──────────▼───────────────┐
    │ Validator Called         │
    │ (Fluent Validation)      │
    │                          │
    │ Rules:                   │
    │ • CitoyenId > 0          │
    │ • Description not empty  │
    │ • Description max 1000   │
    └──────────┬───────────────┘
               │
        ┌──────▼────────┐
        │ Is Valid?     │
        └──┬────────┬───┘
      YES  │        │ NO
           │        └──────────────┐
           │                       │
           ▼                       ▼
    ┌─────────────┐        ┌──────────────────┐
    │Handler runs │        │ValidationException
    │Success      │        │                   │
    │             │        │ Return to user    │
    └─────────────┘        │ Error messages    │
                           └──────────────────┘
```

---

## Conclusion

The architecture is **production-ready** and follows industry best practices:
- ✅ **Separation of Concerns** - Each layer has clear responsibilities
- ✅ **CQRS Pattern** - Commands and Queries separated
- ✅ **Domain-Driven Design** - Focus on domain logic
- ✅ **Testability** - Each layer can be tested independently
- ✅ **Scalability** - Easy to add new features

**You can now build the Razor Pages UI to complete your application!** 🚀
