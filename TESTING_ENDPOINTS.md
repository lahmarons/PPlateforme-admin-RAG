# 🚀 Testing Your CQRS Endpoints

## Getting Started

Your IdaraTech application now has a complete database with all tables created and CQRS commands/queries ready to test.

---

## Starting the Application

### Option 1: From Command Line
```powershell
cd C:\Dev\IdaraTech\src\Web
dotnet run --configuration Debug
```

### Option 2: From Visual Studio
1. Open `C:\Dev\IdaraTech\IdaraTech.slnx`
2. Set `Web` project as startup project
3. Press **F5** to run

**Application will be available at**: https://localhost:7004

---

## Accessing the API

### Swagger UI (Recommended)
Navigate to: **https://localhost:7004/swagger**

This shows all available endpoints with interactive testing interface.

### cURL / Postman
Use the endpoints listed below with proper JSON payloads.

---

## ✅ Test Endpoints Available

### 1️⃣ Create a Demande (Request)

**Endpoint**: `POST /api/demandes`

**Request Body**:
```json
{
  "citoyenId": 1,
  "description": "I need a birth certificate urgently",
  "serviceId": 1
}
```

**Expected Response** (201 Created):
```json
{
  "id": 1,
  "value": 1
}
```

**What Happens**:
- Creates a new Demande entity
- Sets status to "EnAttente" (Pending)
- Publishes `DemandeCreatedEvent`
- Returns the new Demande ID

---

### 2️⃣ Get Demande Details

**Endpoint**: `GET /api/demandes/{id}`

**Example**: `GET /api/demandes/1`

**Expected Response** (200 OK):
```json
{
  "id": 1,
  "citoyenId": 1,
  "citoyenNom": "John Doe",
  "description": "I need a birth certificate urgently",
  "statut": 0,
  "statutName": "EnAttente",
  "serviceId": 1,
  "serviceNom": "Civil Registry",
  "agentId": null,
  "agentNom": null,
  "documents": []
}
```

**What Happens**:
- Retrieves Demande with all related data
- Maps to DTO with flattened structure
- Includes related Citoyen, Service, Agent, and Documents

---

### 3️⃣ Change Demande Status

**Endpoint**: `PUT /api/demandes/{id}/status` *(hypothetical - follow your actual routing)*

**Request Body**:
```json
{
  "demandeId": 1,
  "newStatus": 1
}
```

**Status Values**:
```
0 = EnAttente (Pending)
1 = EnCours (In Progress)
2 = Traitee (Processed/Approved)
3 = Refusee (Refused)
```

**Expected Response** (204 No Content or 200 OK)

**What Happens**:
- Updates Demande status
- Publishes `DemandeStatusChangedEvent`
- Event handlers can send notifications, log changes, etc.

---

## Prerequisites for Testing

Before you can test the endpoints, you need to create initial data:

### Create Test Data

```sql
-- First, insert a Service
INSERT INTO dbo.Services (Nom, Description, Created, CreatedBy, LastModified, LastModifiedBy)
VALUES ('Civil Registry', 'Official birth, marriage, death records', GETUTCDATE(), 'System', GETUTCDATE(), 'System')

-- Then, insert a Citoyen (Citizen)
INSERT INTO dbo.Citoyens (Nom, Email, MotDePasse, Adresse, Telephone, Created, CreatedBy, LastModified, LastModifiedBy)
VALUES ('John Doe', 'john@example.com', 'hashed_password_here', '123 Main St', '555-1234', GETUTCDATE(), 'System', GETUTCDATE(), 'System')

-- Optionally insert an Agent
INSERT INTO dbo.Agents (Nom, Email, MotDePasse, Matricule, Grade, ServiceId, Created, CreatedBy, LastModified, LastModifiedBy)
VALUES ('Jane Smith', 'jane@example.com', 'hashed_password_here', 'AG-001', 'Supervisor', 1, GETUTCDATE(), 'System', GETUTCDATE(), 'System')
```

---

## Testing Workflow

### Step 1: Create a Service
Create a Service via SQL (see above) or create a Command for it (you haven't created this yet)

### Step 2: Create a Citoyen
Create a Citoyen via SQL (see above) or create a Command for it (you haven't created this yet)

### Step 3: Create a Demande
```bash
POST https://localhost:7004/api/demandes
Content-Type: application/json

{
  "citoyenId": 1,
  "description": "I need a birth certificate",
  "serviceId": 1
}
```

**Save the returned ID** (e.g., 1)

### Step 4: Get Demande Details
```bash
GET https://localhost:7004/api/demandes/1
```

You should see:
- The request you created
- Status: "EnAttente"
- All related information

### Step 5: Change Status
```bash
PUT https://localhost:7004/api/demandes/1/status
Content-Type: application/json

{
  "demandeId": 1,
  "newStatus": 1
}
```

### Step 6: Verify Status Change
```bash
GET https://localhost:7004/api/demandes/1
```

You should see:
- Status changed to "EnCours"
- `DemandeStatusChangedEvent` was published

---

## Using Postman

### Import Collection
Create a new Postman collection with these requests:

#### 1. Create Service (SQL - Run in SSMS)
```sql
INSERT INTO dbo.Services (Nom, Description, Created, CreatedBy, LastModified, LastModifiedBy)
VALUES ('Civil Registry', 'Birth records', GETUTCDATE(), 'System', GETUTCDATE(), 'System')
```

#### 2. Create Citizen (SQL - Run in SSMS)
```sql
INSERT INTO dbo.Citoyens (Nom, Email, MotDePasse, Adresse, Telephone, Created, CreatedBy, LastModified, LastModifiedBy)
VALUES ('John Doe', 'john@example.com', 'pwd', '123 Main', '555-1234', GETUTCDATE(), 'System', GETUTCDATE(), 'System')
```

#### 3. Create Demande (Postman)
```
POST https://localhost:7004/api/demandes
Content-Type: application/json

{
  "citoyenId": 1,
  "description": "I need a birth certificate",
  "serviceId": 1
}
```

#### 4. Get Demande (Postman)
```
GET https://localhost:7004/api/demandes/1
```

#### 5. Change Status (Postman)
```
PUT https://localhost:7004/api/demandes/1/status
Content-Type: application/json

{
  "demandeId": 1,
  "newStatus": 1
}
```

---

## Using cURL

```bash
# Set variables
$ApiBase = "https://localhost:7004"
$CitoyenId = 1
$ServiceId = 1

# Create Demande
curl -X POST "$ApiBase/api/demandes" `
  -H "Content-Type: application/json" `
  -d "{`"citoyenId`": $CitoyenId, `"description`": `"Test request`", `"serviceId`": $ServiceId}"

# Get Demande
curl -X GET "$ApiBase/api/demandes/1"

# Change Status
curl -X PUT "$ApiBase/api/demandes/1/status" `
  -H "Content-Type: application/json" `
  -d "{`"demandeId`": 1, `"newStatus`": 1}"
```

---

## Validating CQRS Flow

### When Creating a Demande:
1. ✅ **Validation** - CreateDemandeCommandValidator runs
   - CitoyenId must be > 0
   - Description required and max 1000 chars
2. ✅ **Handler** - CreateDemandeCommandHandler executes
   - Creates entity
   - Publishes DemandeCreatedEvent
   - Saves to database
3. ✅ **Database** - New row inserted into Demandes table

### When Getting Demande:
1. ✅ **Query** - GetDemandeByIdQuery executes
   - Includes related data (Citoyen, Service, Agent, Documents)
2. ✅ **Mapping** - Demandeentity mapped to DemandeDto
3. ✅ **Response** - Structured JSON returned

### When Changing Status:
1. ✅ **Command** - ChangeDemandeStatusCommand validates
2. ✅ **Handler** - ChangeDemandeStatusCommandHandler executes
   - Retrieves entity
   - Calls entity.ChangerStatut(newStatus)
   - Publishes DemandeStatusChangedEvent
3. ✅ **Event** - Handlers can react to status change

---

## Expected Behavior

### Success Scenario
1. Create Service ✅
2. Create Citoyen ✅
3. Create Demande → Returns ID 1 ✅
4. Get Demande 1 → Returns full details ✅
5. Change Status to 1 (EnCours) → Success ✅
6. Get Demande 1 → Status shows "EnCours" ✅

### Error Scenarios

#### Invalid CitoyenId
```json
{
  "citoyenId": 0,
  "description": "Test"
}
```
**Response**: 400 Bad Request (Validation Error)
- Validator rejects CitoyenId ≤ 0

#### Demande Not Found
```
GET /api/demandes/999
```
**Response**: 404 Not Found
- Guard.Against.Null throws NotFoundException

#### Empty Description
```json
{
  "citoyenId": 1,
  "description": ""
}
```
**Response**: 400 Bad Request (Validation Error)
- Validator requires Description

---

## Database Verification

After testing, verify data in database:

```sql
-- Check created Demandes
SELECT * FROM dbo.Demandes;

-- Check Demande with related data
SELECT 
    d.Id, 
    d.Description, 
    d.Statut,
    c.Nom as CitoyenName,
    s.Nom as ServiceName
FROM dbo.Demandes d
LEFT JOIN dbo.Citoyens c ON d.CitoyenId = c.id
LEFT JOIN dbo.Services s ON d.ServiceId = s.Id;

-- Check Documents
SELECT * FROM dbo.Documents;
```

---

## ✨ Features to Notice

### ✅ CQRS Pattern
- **Commands** (CreateDemande, ChangeDemandeStatus) handle writes
- **Queries** (GetDemandeById) handle reads
- Separated concerns

### ✅ Validation
- Automatic via FluentValidation
- Happens before handler executes
- Clear error messages

### ✅ Domain Events
- DemandeCreatedEvent published on create
- DemandeStatusChangedEvent published on status change
- Event handlers can react (email, notifications, etc.)

### ✅ DTOs
- Domain entities never exposed directly
- DTOs flatten related data
- Safe for API responses

### ✅ Async/Await
- All database operations are async
- Scalable and performant
- Non-blocking I/O

---

## Next: Create UI Endpoints

You should also implement endpoints to create Users and Services:

```csharp
// Create these Commands/Handlers next:
- CreateCitoyenCommand
- CreateAgentCommand
- CreateServiceCommand
- UpdateCitoyenCommand
- DeleteCitoyenCommand
```

---

## 📞 Quick Reference

| Action | HTTP Method | Endpoint | Status |
|--------|-------------|----------|--------|
| Create Demande | POST | /api/demandes | ✅ |
| Get Demande | GET | /api/demandes/{id} | ✅ |
| Change Status | PUT | /api/demandes/{id}/status | ✅ |
| Create Citoyen | POST | /api/citoyens | 🔲 TODO |
| List Demandes | GET | /api/demandes | 🔲 TODO |
| Get Services | GET | /api/services | 🔲 TODO |

---

## 🎉 You're Ready!

1. ✅ Start the application: `dotnet run`
2. ✅ Open Swagger: https://localhost:7004/swagger
3. ✅ Create test data (via SQL)
4. ✅ Test the endpoints
5. ✅ Verify CQRS flow works

**Happy testing!** 🚀
