---
name: dotnet-clean-endpoint
description: >
  Generate minimal API endpoints for ASP.NET Core Clean Architecture projects
  using the class-per-endpoint pattern. Each endpoint is a class implementing
  IEndpoint, organized in Endpoints/{Entity}/ folders. Covers CRUD generation,
  auto-registration via assembly scanning, request/response DTOs, and route
  conventions. Trigger whenever the user wants to add an endpoint, wire up a new route,
  or register HTTP routes for an entity — including Vietnamese like "tạo endpoint",
  "thêm API cho entity", "thêm route", "đăng ký route", "tạo CRUD endpoint".
  Do NOT trigger for entity generation, or project scaffolding.
  Also invoke during /speckit.plan when designing API contracts or HTTP surface,
  and during /speckit.tasks or /speckit.implement for tasks under
  src/{Project}.Api/Endpoints/ or mentioning HTTP routes or endpoints.
---

# Minimal API Endpoints — Class-per-Endpoint Pattern

## Step 1 — One-time Infrastructure

Create these files if they don't exist. Full code in `references/endpoint-infrastructure.md`.

| File | Purpose |
|------|---------|
| `src/{Project}.API/Endpoints/IEndpoint.cs` | Interface with `MapEndpoint(IEndpointRouteBuilder)` |
| `src/{Project}.API/Endpoints/EndpointExtensions.cs` | Scans assembly, auto-registers all `IEndpoint` classes |
| `Program.cs` | Replace manual mapping with `app.MapEndpoints()` |

## Step 2 — Generate Endpoint Classes

Folder: `src/{Project}.API/Endpoints/{Entity}/`. One class per HTTP operation. Full templates in `references/endpoint-templates.md`.

**Standard CRUD set:**

| Class | Route | Method |
|-------|-------|--------|
| `GetAll{Entity}Endpoint` | `/api/{entities}` | GET |
| `Get{Entity}ByIdEndpoint` | `/api/{entities}/{id:guid}` | GET |
| `Create{Entity}Endpoint` | `/api/{entities}` | POST |
| `Update{Entity}Endpoint` | `/api/{entities}` | PUT |
| `Delete{Entity}Endpoint` | `/api/{entities}/{id:guid}` | DELETE |

## Step 3 — Request / Response Binding

**Never create separate request DTOs in the endpoint.** Bind directly to Application layer Commands/Queries.

| Method | Lambda signature | Notes |
|--------|-----------------|-------|
| POST | `(CreateXCommand command, ISender sender)` | Body → Command |
| PUT/PATCH | `(UpdateXCommand command, ISender sender)` | Body includes `Id` — no route `{id}` |
| DELETE | `(Guid id, ISender sender)` | Construct `new DeleteXCommand(id)` inline |
| GET list | `([AsParameters] GetXsQuery query, ISender sender)` | Query string → Query record |
| GET by id | `(Guid id, ISender sender)` | Construct `new GetXByIdQuery(id)` inline |

**Response pattern:**
```csharp
// Result<T> — with value
var result = await sender.Send(command);
return result.IsSuccess
    ? Results.Created($"/api/products/{result.Value.Id}", result.Value)
    : Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                      statusCode: StatusCodes.Status400BadRequest);

// Result (void) — no value
return result.IsSuccess
    ? Results.NoContent()
    : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
```

## Step 4 — OpenAPI (mandatory on every endpoint)

Every route MUST end with `.WithName()`, `.WithTags()`, and all `.Produces<T>()` calls:

| Method | Required Produces |
|--------|------------------|
| GET list | `.Produces<IEnumerable<T>>()` + `500` |
| GET by id | `.Produces<T>()` + `404` + `500` |
| POST | `.Produces<T>(201)` + `400` + `500` |
| PUT/PATCH | `.Produces<T>()` + `404` + `400` + `500` |
| DELETE | `.Produces(204)` + `400` + `404` + `500` |

Use `StatusCodes.StatusXXX` constants in code. Add extra codes if the handler returns them (409, 422…).

## Step 5 — Validate

1. All endpoint files in correct folders
2. `IEndpoint` + `EndpointExtensions` exist
3. `Program.cs` calls `app.MapEndpoints()`
4. `dotnet build` passes

## Key Rules

- **One class = one HTTP operation.** Never put multiple Map* calls in one class.
- **DI via delegate params**, not constructor — the class is instantiated once at startup; runtime deps (ISender, repos) go in the lambda.
- **Routes**: plural, lowercase, kebab-case (`/api/order-items` for `OrderItem`).
- **Before writing**: read the Command/Query file to confirm class name, return type (`Result` vs `Result<T>`), and Response DTO name.
- **Target .NET 10 / C# 13**: file-scoped namespaces, `internal sealed class`.
- If entity name is in Vietnamese, translate to English PascalCase and confirm with user.
