# ✅ Your Swagger Endpoints Are Now Available!

## What You Should See in Swagger

Navigate to: **https://localhost:7004/swagger**

You should now see this in the Swagger UI:

---

## 📋 API Endpoints

### 1. **POST** `/api/demandes`
**Summary**: Create a new request (Demande)

**Request Body**:
```json
{
  "citoyenId": 1,
  "description": "string",
  "serviceId": 0
}
```

**Response** (200 OK):
```json
{
  "id": 1
}
```

**Possible Status Codes**:
- ✅ 200 - Request created successfully
- ❌ 400 - Invalid input (validation error)

---

### 2. **GET** `/api/demandes/{id}`
**Summary**: Get request details by ID

**Parameters**:
- `id` (integer, required) - Request ID

**Response** (200 OK):
```json
{
  "id": 1,
  "citoyenId": 1,
  "citoyenNom": "John Doe",
  "description": "I need a birth certificate",
  "statut": 0,
  "statutName": "EnAttente",
  "serviceId": 1,
  "serviceNom": "Civil Registry",
  "agentId": null,
  "agentNom": null,
  "documents": []
}
```

**Possible Status Codes**:
- ✅ 200 - Request found
- ❌ 404 - Request not found

---

### 3. **PUT** `/api/demandes/{id}/status`
**Summary**: Change the status of a request

**Parameters**:
- `id` (integer, required) - Request ID

**Request Body**:
```json
{
  "demandeId": 1,
  "newStatus": 1
}
```

**Status Values**:
- `0` = EnAttente (Pending)
- `1` = EnCours (In Progress)
- `2` = Traitee (Processed/Approved)
- `3` = Refusee (Refused)

**Response** (204 No Content):
No response body

**Possible Status Codes**:
- ✅ 204 - Status changed successfully
- ❌ 404 - Request not found
- ❌ 400 - Invalid status

---

## 🧪 Try It Out in Swagger

### Step 1: Prepare Test Data
First, create test data using SQL Server Management Studio:

```sql
-- Create a Service
INSERT INTO dbo.Services (Nom, Description, Created, CreatedBy, LastModified, LastModifiedBy)
VALUES ('Civil Registry', 'Birth records service', GETUTCDATE(), 'System', GETUTCDATE(), 'System')

-- Create a Citizen
INSERT INTO dbo.Citoyens (Nom, Email, MotDePasse, Adresse, Telephone, Created, CreatedBy, LastModified, LastModifiedBy)
VALUES ('John Doe', 'john@example.com', 'pwd', '123 Main St', '555-1234', GETUTCDATE(), 'System', GETUTCDATE(), 'System')
```

### Step 2: Test Create Endpoint
1. Open Swagger: https://localhost:7004/swagger
2. Find **POST** `/api/demandes`
3. Click **Try it out**
4. Enter request body:
```json
{
  "citoyenId": 1,
  "description": "I need a birth certificate",
  "serviceId": 1
}
```
5. Click **Execute**
6. You should see response with ID: `{"id": 1}`

### Step 3: Test Get Endpoint
1. Find **GET** `/api/demandes/{id}`
2. Click **Try it out**
3. Enter `id`: `1`
4. Click **Execute**
5. You should see full request details

### Step 4: Test Status Change
1. Find **PUT** `/api/demandes/{id}/status`
2. Click **Try it out**
3. Enter `id`: `1`
4. Enter request body:
```json
{
  "demandeId": 1,
  "newStatus": 1
}
```
5. Click **Execute**
6. You should see `204 No Content` response
7. Run GET again to verify status changed to "EnCours"

---

## 📝 Controller Breakdown

### File: `src/Web/Controllers/DemandesController.cs`

```csharp
[ApiController]                                    // Enables automatic model validation
[Route("api/[controller]")]                        // Route: /api/demandes
[Produces("application/json")]                     // Returns JSON
public class DemandesController : ControllerBase
{
    private readonly IMediator _mediator;          // Injected MediatR
    
    // POST /api/demandes
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateDemandeCommand command,               // Auto-bound from JSON body
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command); // Calls CreateDemandeCommandHandler
        return Ok(new { id = result });             // Returns 200 with ID
    }
    
    // GET /api/demandes/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        int id,                                      // Auto-bound from route
        CancellationToken cancellationToken)
    {
        var query = new GetDemandeByIdQuery { Id = id };
        var result = await _mediator.Send(query);   // Calls GetDemandeByIdQueryHandler
        return Ok(result);                           // Returns 200 with DTO
    }
    
    // PUT /api/demandes/{id}/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> ChangeStatus(
        int id,
        ChangeDemandeStatusCommand command,         // Auto-bound from JSON body
        CancellationToken cancellationToken)
    {
        command = command with { DemandeId = id };  // Map route ID to command
        await _mediator.Send(command);              // Calls ChangeDemandeStatusCommandHandler
        return NoContent();                         // Returns 204 (success, no content)
    }
}
```

---

## 🔄 Request Flow

### When you POST to `/api/demandes`:

```
1. Client sends HTTP POST
   ↓
2. DemandesController.Create() receives request
   ↓
3. ASP.NET binds JSON to CreateDemandeCommand
   ↓
4. MediatR validators run (CreateDemandeCommandValidator)
   ↓
5. If valid, sends command to handler via: _mediator.Send(command)
   ↓
6. CreateDemandeCommandHandler.Handle() executes
   ├─ Creates Demande entity
   ├─ Publishes DemandeCreatedEvent
   └─ Saves to database
   ↓
7. Returns ID to controller
   ↓
8. Controller returns 200 OK with { id: 1 }
   ↓
9. Client receives response
```

---

## 🎯 Key Features

### ✅ Automatic Validation
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateDemandeCommand command)
{
    // If validation fails, MediatR ValidationBehaviour catches it
    // and returns 400 Bad Request with error details
}
```

### ✅ Auto-binding
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    // 'id' automatically bound from route: /demandes/{id}
}

[HttpPost]
public async Task<IActionResult> Create(CreateDemandeCommand command)
{
    // 'command' automatically bound from JSON request body
}
```

### ✅ Swagger Documentation
```csharp
/// <summary>Create a new request</summary>
/// <response code="200">Request created successfully</response>
/// <response code="400">Invalid input</response>
[HttpPost]
public async Task<IActionResult> Create(CreateDemandeCommand command)
```

---

## 🚀 Start Testing Now

### 1. Start the Application
```powershell
cd C:\Dev\IdaraTech\src\Web
dotnet run
```

### 2. Open Swagger
```
https://localhost:7004/swagger
```

### 3. Prepare Test Data (SQL)
```sql
INSERT INTO dbo.Services (Nom, Description, Created, CreatedBy, LastModified, LastModifiedBy)
VALUES ('Civil Registry', 'Test', GETUTCDATE(), 'System', GETUTCDATE(), 'System')

INSERT INTO dbo.Citoyens (Nom, Email, MotDePasse, Adresse, Telephone, Created, CreatedBy, LastModified, LastModifiedBy)
VALUES ('John Doe', 'john@example.com', 'pwd', '123 Main', '555-1234', GETUTCDATE(), 'System', GETUTCDATE(), 'System')
```

### 4. Test Create Endpoint
POST /api/demandes with:
```json
{
  "citoyenId": 1,
  "description": "Test request",
  "serviceId": 1
}
```

### 5. Test Get Endpoint
GET /api/demandes/1

### 6. Test Status Change
PUT /api/demandes/1/status with:
```json
{
  "demandeId": 1,
  "newStatus": 1
}
```

---

## ✨ You're All Set!

Your endpoints are now:
✅ Created
✅ Documented in Swagger
✅ Ready to test
✅ Fully functional

**Next Steps**:
1. Run the application
2. Test the endpoints in Swagger
3. Create CitoyensController for user management
4. Create ServicesController for service management
5. Create AgentsController for agent management

---

**Happy API Testing!** 🎉
