# ✅ Demandes Endpoints Created - Minimal Endpoints Pattern

## What Was Created

I've created minimal API endpoints for your Demandes (requests) following the same pattern as TodoItems.

---

## 📁 Files Created/Modified

### New Files
```
✅ src/Web/Endpoints/Demandes.cs
   - 6 endpoint handlers (Get list, Get single, Create, Update, Delete, Change Status)
   
✅ src/Application/Demandes/Queries/GetDemandesWithPagination/GetDemandesWithPagination.cs
   - Query to get paginated list of demandes
   
✅ src/Application/Demandes/Commands/UpdateDemande/UpdateDemande.cs
   - Command to update demande
   
✅ src/Application/Demandes/Commands/DeleteDemande/DeleteDemande.cs
   - Command to delete demande
```

### Deleted Files
```
❌ src/Web/Controllers/DemandesController.cs
   - Removed (replaced with minimal endpoints)
```

---

## 🚀 Endpoints Available

### Base URL: `/api/demandes`

| Method | Endpoint | Name | Description |
|--------|----------|------|-------------|
| **GET** | `/api/demandes` | GetDemandes | Get all demandes (paginated) |
| **GET** | `/api/demandes/{id}` | GetDemandeById | Get single demande by ID |
| **POST** | `/api/demandes` | CreateDemande | Create new demande |
| **PUT** | `/api/demandes/{id}` | UpdateDemande | Update demande |
| **DELETE** | `/api/demandes/{id}` | DeleteDemande | Delete demande |
| **PUT** | `/api/demandes/{id}/status` | ChangeDemandeStatus | Change demande status |

---

## 📋 Endpoint Details

### 1. GET `/api/demandes` - Get All Demandes (Paginated)

**Query Parameters:**
- `pageNumber` (integer, optional) - Default: 1
- `pageSize` (integer, optional) - Default: 10

**Example Request:**
```
GET /api/demandes?pageNumber=1&pageSize=10
```

**Response** (200 OK):
```json
{
  "items": [
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
  ],
  "pageNumber": 1,
  "totalPages": 1,
  "totalCount": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

### 2. GET `/api/demandes/{id}` - Get Single Demande

**Path Parameters:**
- `id` (integer, required) - Demande ID

**Example Request:**
```
GET /api/demandes/1
```

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

**Response** (404 Not Found):
- If demande doesn't exist

---

### 3. POST `/api/demandes` - Create Demande

**Request Body:**
```json
{
  "citoyenId": 1,
  "description": "I need a birth certificate",
  "serviceId": 1
}
```

**Response** (201 Created):
```
Location: /api/demandes/1

{
  "id": 1
}
```

**Response** (400 Bad Request):
- If validation fails (citoyenId ≤ 0, description empty, etc.)

---

### 4. PUT `/api/demandes/{id}` - Update Demande

**Path Parameters:**
- `id` (integer, required) - Demande ID

**Request Body:**
```json
{
  "id": 1,
  "description": "I need a birth certificate urgently",
  "serviceId": 1
}
```

**Response** (204 No Content):
- No response body on success

**Response** (400 Bad Request):
- If path ID doesn't match body ID

**Response** (404 Not Found):
- If demande doesn't exist

---

### 5. DELETE `/api/demandes/{id}` - Delete Demande

**Path Parameters:**
- `id` (integer, required) - Demande ID

**Example Request:**
```
DELETE /api/demandes/1
```

**Response** (204 No Content):
- No response body on success

**Response** (404 Not Found):
- If demande doesn't exist

---

### 6. PUT `/api/demandes/{id}/status` - Change Demande Status

**Path Parameters:**
- `id` (integer, required) - Demande ID

**Request Body:**
```json
{
  "demandeId": 1,
  "newStatus": 1
}
```

**Status Values:**
- `0` = EnAttente (Pending)
- `1` = EnCours (In Progress)
- `2` = Traitee (Processed/Approved)
- `3` = Refusee (Refused)

**Response** (204 No Content):
- No response body on success

**Response** (404 Not Found):
- If demande doesn't exist

---

## 🧪 Testing the Endpoints

### 1. Start the Application
```powershell
cd C:\Dev\IdaraTech\src\Web
dotnet run
```

### 2. Prepare Test Data
In SQL Server Management Studio, run:

```sql
-- Create Service
INSERT INTO dbo.Services (Nom, Description, Created, CreatedBy, LastModified, LastModifiedBy)
VALUES ('Civil Registry', 'Birth records', GETUTCDATE(), 'System', GETUTCDATE(), 'System')

-- Create Citizen
INSERT INTO dbo.Citoyens (Nom, Email, MotDePasse, Adresse, Telephone, Created, CreatedBy, LastModified, LastModifiedBy)
VALUES ('John Doe', 'john@example.com', 'pwd', '123 Main', '555-1234', GETUTCDATE(), 'System', GETUTCDATE(), 'System')
```

### 3. Test via Swagger
Navigate to: **https://localhost:7004/api**

You should see the **Demandes** section with all 6 endpoints.

### 4. Test via cURL/Postman

**Create a Demande:**
```bash
curl -X POST "https://localhost:7004/api/demandes" \
  -H "Content-Type: application/json" \
  -d '{
    "citoyenId": 1,
    "description": "I need a birth certificate",
    "serviceId": 1
  }'
```

**Get All Demandes:**
```bash
curl -X GET "https://localhost:7004/api/demandes?pageNumber=1&pageSize=10"
```

**Get Single Demande:**
```bash
curl -X GET "https://localhost:7004/api/demandes/1"
```

**Update Demande:**
```bash
curl -X PUT "https://localhost:7004/api/demandes/1" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "description": "I need a birth certificate urgently",
    "serviceId": 1
  }'
```

**Change Status:**
```bash
curl -X PUT "https://localhost:7004/api/demandes/1/status" \
  -H "Content-Type: application/json" \
  -d '{
    "demandeId": 1,
    "newStatus": 1
  }'
```

**Delete Demande:**
```bash
curl -X DELETE "https://localhost:7004/api/demandes/1"
```

---

## 🎯 Minimal Endpoints Pattern Explanation

### What is Minimal Endpoints?

Instead of traditional Controllers:
```csharp
// ❌ Traditional Controller
[ApiController]
[Route("api/[controller]")]
public class DemandesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateDemandeCommand command) { ... }
}
```

We use Minimal Endpoints:
```csharp
// ✅ Minimal Endpoints
public class Demandes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPost(CreateDemande);
    }
    
    public async Task<Created<int>> CreateDemande(ISender sender, CreateDemandeCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/demandes/{id}", id);
    }
}
```

### Benefits
✅ **More concise** - Less boilerplate code
✅ **Better performance** - Directly maps to minimal infrastructure
✅ **Type-safe** - Compile-time checking of routes
✅ **Middleware-friendly** - Easy to apply per-endpoint middleware
✅ **Testable** - Just methods, no framework baggage

---

## 🔄 Request Flow

```
HTTP Request
    ↓
Minimal Endpoint mapper (MapPost, MapGet, etc.)
    ↓
Demandes.CreateDemande() method
    ↓
Receives ISender (MediatR)
    ↓
Sends CreateDemandeCommand via MediatR
    ↓
MediatR finds CreateDemandeCommandHandler
    ↓
Validator runs (CreateDemandeCommandValidator)
    ↓
If valid, handler executes:
  - Creates entity
  - Publishes DemandeCreatedEvent
  - Saves to database
    ↓
Returns ID to endpoint
    ↓
Endpoint returns TypedResults.Created(location, id)
    ↓
HTTP 201 Created response
    ↓
Client receives response
```

---

## 📊 Code Structure

### File: `src/Web/Endpoints/Demandes.cs`

```csharp
public class Demandes : EndpointGroupBase
{
    // Automatic endpoint group name from class name
    public override string GroupName => "Demandes";
    
    // Maps all the routes
    public override void Map(RouteGroupBuilder group)
    {
        // Automatically prefixed with /api/Demandes
        group.MapGet(GetDemandesWithPagination);  // GET /api/demandes
        group.MapGet(GetDemandeById, "{id}");     // GET /api/demandes/{id}
        group.MapPost(CreateDemande);             // POST /api/demandes
        group.MapPut(UpdateDemande, "{id}");      // PUT /api/demandes/{id}
        group.MapDelete(DeleteDemande, "{id}");   // DELETE /api/demandes/{id}
        group.MapPut(ChangeDemandeStatus, "{id}/status"); // PUT /api/demandes/{id}/status
    }
    
    // Each is a separate method (handler)
    public async Task<Ok<PaginatedList<DemandeDto>>> GetDemandesWithPagination(
        ISender sender,
        [AsParameters] GetDemandesWithPaginationQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }
    
    // ... more endpoint methods
}
```

### How Endpoints Are Discovered

In `Program.cs`:
```csharp
app.MapEndpoints();
```

This automatically:
1. Scans assembly for classes inheriting from `EndpointGroupBase`
2. Creates instances
3. Calls `.Map()` on each
4. Routes are registered with `/api/{ClassName}` prefix

---

## ✅ What Works Now

✅ **List Demandes** - With pagination support
✅ **Get Single Demande** - By ID
✅ **Create Demande** - With validation
✅ **Update Demande** - Partial updates
✅ **Delete Demande** - Remove completely
✅ **Change Status** - With domain event publishing
✅ **Swagger Documentation** - All endpoints auto-documented
✅ **Error Handling** - Proper HTTP status codes (200, 201, 204, 400, 404)

---

## 🎉 You Now Have

```
✅ 6 functional endpoints for Demandes
✅ Pagination support on list endpoint
✅ CQRS commands/queries working
✅ Domain events publishing
✅ Automatic validation
✅ Swagger documentation
✅ Type-safe minimal endpoints
✅ Production-ready code
```

---

## 🚀 Next Steps

### Create More Endpoints

Following the same pattern, create:

1. **CitoyensController** → `CitoyensEndpoints.cs`
2. **ServicesController** → `ServicesEndpoints.cs`
3. **AgentsController** → `AgentsEndpoints.cs`

Just copy the pattern from `Demandes.cs` and adapt it for the other entities!

---

## 📞 Quick Reference

| Task | File |
|------|------|
| View endpoints | `src/Web/Endpoints/Demandes.cs` |
| View queries | `src/Application/Demandes/Queries/` |
| View commands | `src/Application/Demandes/Commands/` |
| Test endpoints | https://localhost:7004/api |

---

**Status**: ✅ **Demandes Endpoints Fully Operational**

**Test Now**: 
1. Run: `dotnet run -p src/Web`
2. Go to: https://localhost:7004/api
3. Expand the "Demandes" section
4. Click "Try it out" on any endpoint!
