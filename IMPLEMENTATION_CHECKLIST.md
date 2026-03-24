# ✅ IdaraTech Implementation Checklist

## Phase 1: Foundation (COMPLETED ✅)

### Domain Layer
- [x] Create base `Utilisateur` entity (abstract)
- [x] Create `Agent` entity (inherits Utilisateur)
- [x] Create `Citoyen` entity (inherits Utilisateur)
- [x] Create `Admin` entity (inherits Utilisateur)
- [x] Create `Demande` entity (request)
- [x] Create `Service` entity
- [x] Create `Document` entity
- [x] Create `Statut` enumeration
- [x] Create `DemandeCreatedEvent` domain event
- [x] Create `DemandeStatusChangedEvent` domain event
- [x] Create `UtilisateurCreatedEvent` domain event

### Application Layer
- [x] Create `CreateDemandeCommand` with validator
- [x] Create `CreateDemandeCommandHandler`
- [x] Create `CreateDemandeCommandValidator`
- [x] Create `ChangeDemandeStatusCommand` with handler
- [x] Create `GetDemandeByIdQuery` with handler
- [x] Create `DemandeDto` DTO
- [x] Create `DocumentDto` DTO
- [x] Create `DemandeMappingProfile` (AutoMapper)
- [x] Update `IApplicationDbContext` interface with all DbSets
- [x] Create CQRS command handler base class (if needed)

### Infrastructure Layer
- [x] Create `UtilisateurConfiguration` (EF Fluent API)
- [x] Create `AgentConfiguration` (EF Fluent API)
- [x] Create `CitoyenConfiguration` (EF Fluent API)
- [x] Create `AdminConfiguration` (EF Fluent API)
- [x] Create `DemandeConfiguration` (EF Fluent API)
- [x] Create `ServiceConfiguration` (EF Fluent API)
- [x] Create `DocumentConfiguration` (EF Fluent API)
- [x] Update `ApplicationDbContext` with new DbSets
- [x] Verify all relationships are configured correctly
- [x] Verify cascade delete behaviors

### Compilation
- [x] All Domain entities compile
- [x] All Application handlers compile
- [x] All Infrastructure configs compile
- [x] No namespace errors
- [x] No missing using statements
- [x] Guard clauses used correctly

---

## Phase 2: Database Setup (NEXT STEP)

### Create Migration
- [ ] Run: `dotnet ef migrations add AddUtilisateurEntities --project src/Infrastructure --startup-project src/Web`
- [ ] Check generated migration file
- [ ] Verify all entities are in migration
- [ ] Verify foreign keys are correct

### Apply Migration
- [ ] Run: `dotnet ef database update --project src/Infrastructure --startup-project src/Web`
- [ ] Verify database was created/updated
- [ ] Check database schema in SQL Server Management Studio
- [ ] Verify all tables created
- [ ] Verify all columns match configuration

### Database Verification
- [ ] [ ] Table `dbo.Utilisateurs` exists with correct columns
- [ ] [ ] Table `dbo.Agents` exists with FK to Utilisateurs
- [ ] [ ] Table `dbo.Citoyens` exists with FK to Utilisateurs
- [ ] [ ] Table `dbo.Admins` exists with FK to Utilisateurs
- [ ] [ ] Table `dbo.Demandes` exists with correct FKs
- [ ] [ ] Table `dbo.Services` exists
- [ ] [ ] Table `dbo.Documents` exists with FK to Demandes
- [ ] [ ] Junction table `dbo.AdminService` exists
- [ ] [ ] All indexes created
- [ ] [ ] Constraints properly set

---

## Phase 3: Testing (RECOMMENDED)

### Unit Tests
- [ ] Create test for `CreateDemandeCommand`
- [ ] Create test for `CreateDemandeCommandValidator`
- [ ] Create test for `ChangeDemandeStatusCommand`
- [ ] Create test for `GetDemandeByIdQuery`
- [ ] Test valid command execution
- [ ] Test invalid command validation
- [ ] Test domain event publishing

### Integration Tests
- [ ] Test database insert for Demande
- [ ] Test database update for status change
- [ ] Test foreign key constraints
- [ ] Test cascade delete behavior
- [ ] Test relationship loading

### Database Tests
- [ ] Seed test data
- [ ] Verify LINQ queries work
- [ ] Verify eager loading works
- [ ] Verify filtering works
- [ ] Verify sorting works

---

## Phase 4: Razor Pages (UI LAYER)

### Create Pages Structure
- [ ] Create folder: `Pages/Demandes/`
- [ ] Create: `Pages/Demandes/Index.cshtml` (list view)
- [ ] Create: `Pages/Demandes/Create.cshtml` (create form)
- [ ] Create: `Pages/Demandes/Details.cshtml` (view detail)
- [ ] Create: `Pages/Demandes/Edit.cshtml` (edit form)
- [ ] Create: `Pages/Demandes/Delete.cshtml` (delete confirm)

### Page Models
- [ ] Create: `Pages/Demandes/IndexModel.cs` with `GetDemandes` handler
- [ ] Create: `Pages/Demandes/CreateModel.cs` with `OnPostAsync` handler
- [ ] Create: `Pages/Demandes/DetailsModel.cs` with `OnGetAsync` handler
- [ ] Create: `Pages/Demandes/EditModel.cs` with `OnPostAsync` handler
- [ ] Create: `Pages/Demandes/DeleteModel.cs` with `OnPostAsync` handler
- [ ] Inject `IMediator` into each page model
- [ ] Send CQRS commands/queries using MediatR

### Page Content
- [ ] Index: Display list of all demandes (paginated)
- [ ] Create: Form to submit new demande
- [ ] Details: Show demande with all details and documents
- [ ] Edit: Form to update demande info
- [ ] Delete: Confirmation before deletion
- [ ] Add status badge styling
- [ ] Add validation error messages
- [ ] Add success/error notifications

### Navigation
- [ ] Add menu item for Demandes
- [ ] Add breadcrumb navigation
- [ ] Add back/cancel buttons
- [ ] Add links between pages
- [ ] Add button styling (Bootstrap/CSS)

---

## Phase 5: User Management (OPTIONAL)

### User Management Pages
- [ ] Create folder: `Pages/Users/`
- [ ] Create: `Pages/Users/Index.cshtml` (list users)
- [ ] Create: `Pages/Users/Create.cshtml` (create user)
- [ ] Create: `Pages/Users/Details.cshtml`
- [ ] Create: `Pages/Users/Edit.cshtml`

### User Commands (to create)
- [ ] `CreateCitoyenCommand`
- [ ] `CreateAgentCommand`
- [ ] `CreateAdminCommand`
- [ ] `UpdateUtilisateurCommand`
- [ ] `DeleteUtilisateurCommand`

### User Queries (to create)
- [ ] `GetCitoyensQuery`
- [ ] `GetAgentsQuery`
- [ ] `GetAdminsQuery`
- [ ] `GetUtilisateurByIdQuery`

---

## Phase 6: Service Management (OPTIONAL)

### Service Pages
- [ ] Create folder: `Pages/Services/`
- [ ] Create: `Pages/Services/Index.cshtml`
- [ ] Create: `Pages/Services/Create.cshtml`
- [ ] Create: `Pages/Services/Edit.cshtml`

### Service Commands (to create)
- [ ] `CreateServiceCommand`
- [ ] `UpdateServiceCommand`
- [ ] `DeleteServiceCommand`

### Service Queries (to create)
- [ ] `GetServicesQuery`
- [ ] `GetServiceByIdQuery`

---

## Phase 7: Event Handlers (RECOMMENDED)

### Create Event Handlers
- [ ] `DemandeCreatedEventHandler`
  - [ ] Send email notification to admin
  - [ ] Create audit log
  - [ ] Update statistics
  
- [ ] `DemandeStatusChangedEventHandler`
  - [ ] Send email to citoyen about status change
  - [ ] Send email to agent if assigned
  - [ ] Create activity log
  
- [ ] `UtilisateurCreatedEventHandler`
  - [ ] Send welcome email
  - [ ] Create initial profile
  - [ ] Log user creation

### Email Service
- [ ] Create `IEmailService` interface
- [ ] Implement email sending service
- [ ] Configure email templates
- [ ] Test email functionality

---

## Phase 8: Advanced Features (FUTURE)

### Document Management
- [ ] Implement file upload for documents
- [ ] Store files in blob storage (Azure/S3)
- [ ] Generate file download URLs
- [ ] Implement file type validation
- [ ] Add virus scanning

### Reporting & Analytics
- [ ] Create dashboard page
- [ ] Show demande statistics
- [ ] Show processing time metrics
- [ ] Generate PDF reports
- [ ] Export data to Excel

### Notifications
- [ ] Email notifications (done in Phase 7)
- [ ] SMS notifications
- [ ] Push notifications
- [ ] In-app notifications

### Approval Workflow
- [ ] Implement multi-step approval
- [ ] Add approval rules engine
- [ ] Create approval queue
- [ ] Add comments/notes on approval

### Search & Filtering
- [ ] Implement demande search
- [ ] Add advanced filtering
- [ ] Add sorting
- [ ] Add pagination
- [ ] Add search indexing

---

## Phase 9: Security & Performance

### Security
- [ ] Implement role-based access control (RBAC)
- [ ] Add authorization attributes to pages
- [ ] Implement data encryption for sensitive fields
- [ ] Add audit logging
- [ ] Implement rate limiting
- [ ] Add CSRF protection
- [ ] Input validation on all pages

### Performance
- [ ] Add caching strategy
- [ ] Optimize database queries
- [ ] Add indexes where needed
- [ ] Implement lazy loading
- [ ] Profile slow queries
- [ ] Add async all the way
- [ ] Implement pagination
- [ ] Compress responses

### Monitoring
- [ ] Add application logging
- [ ] Implement health checks
- [ ] Add performance monitoring
- [ ] Create alerts for errors
- [ ] Monitor database performance

---

## Phase 10: Deployment

### Pre-Deployment
- [ ] Run full test suite
- [ ] Code review
- [ ] Security scan
- [ ] Performance testing
- [ ] Load testing
- [ ] Database migration testing

### Deployment
- [ ] Set up staging environment
- [ ] Deploy to staging
- [ ] Run smoke tests
- [ ] Deploy to production
- [ ] Monitor for errors
- [ ] Get stakeholder approval

### Post-Deployment
- [ ] Monitor error logs
- [ ] Monitor performance
- [ ] Gather user feedback
- [ ] Document deployment
- [ ] Create rollback plan
- [ ] Plan next release

---

## Quick Reference: What's Complete

### ✅ Already Done (Phase 1)
```
✅ 7 Domain Entities
✅ 1 Enumeration
✅ 3 Domain Events
✅ 2 CQRS Commands
✅ 1 CQRS Query
✅ 2 DTOs
✅ 7 EF Configurations
✅ Updated DbContext
✅ Updated Interface
✅ AutoMapper Profile
```

### 📝 Next Steps (Phase 2)
```
1. Create migration
   dotnet ef migrations add AddUtilisateurEntities --project src/Infrastructure --startup-project src/Web

2. Apply migration
   dotnet ef database update --project src/Infrastructure --startup-project src/Web

3. Verify database created correctly

4. Build solution to confirm no errors
```

### 🎯 Then Build UI (Phase 4)
```
Create Razor Pages for:
- Demandes (Create, Read, Update, Delete, List)
- Services (Create, Read, Update, Delete, List)
- Users (Create, Read, Update, Delete, List)
```

---

## File Locations Reference

| Item | Location |
|------|----------|
| Base Entity | `src/Domain/Entities/Utilisateur.cs` |
| Status Enum | `src/Domain/Enums/Statut.cs` |
| Domain Events | `src/Domain/Events/*.cs` |
| Commands | `src/Application/Demandes/Commands/` |
| Queries | `src/Application/Demandes/Queries/` |
| DTOs | `src/Application/Demandes/Queries/GetDemandById/DemandeDto.cs` |
| EF Config | `src/Infrastructure/Data/Configurations/` |
| DbContext | `src/Infrastructure/Data/ApplicationDbContext.cs` |
| Interface | `src/Application/Common/Interfaces/IApplicationDbContext.cs` |
| Mappings | `src/Application/Common/Mappings/DemandeMappingProfile.cs` |

---

## Key Resources

1. **Architecture Guide**: `CLASS_STRUCTURE_GUIDE.md`
2. **Quick Start**: `QUICK_START.md`
3. **Diagrams**: `ARCHITECTURE_DIAGRAMS.md`
4. **Implementation**: `IMPLEMENTATION_COMPLETE.md`
5. **Summary**: `README_IMPLEMENTATION.md`

---

## 🎉 You're Ready!

**Status: Phase 1 Complete ✅**

All foundation work is done. You can now:
1. Create database migration
2. Apply migration to database
3. Start building Razor Pages UI
4. Wire up CQRS commands/queries

**Estimated time to completion:**
- Database setup: 15 minutes
- Razor Pages for CRUD: 2-3 hours
- Event handlers: 1 hour
- Testing: 2-3 hours
- Deployment: 1 hour

**Total: ~8-9 hours for a complete working application**

Good luck! 🚀
