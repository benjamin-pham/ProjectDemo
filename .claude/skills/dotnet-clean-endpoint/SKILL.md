---
name: dotnet-clean-endpoint
description: >
  Generate minimal API endpoints for ASP.NET Core Clean Architecture projects
  using the class-per-endpoint pattern. Each endpoint is a class implementing
  IEndpoint, co-located with its Command/Query in the Application layer.
  Covers CRUD generation, auto-registration via DI assembly scanning, request/response
  DTOs, and route conventions. Trigger whenever the user wants to add an endpoint,
  wire up a new route, or register HTTP routes for an entity — including Vietnamese
  like "tạo endpoint", "thêm API cho entity", "thêm route", "đăng ký route", "tạo CRUD endpoint".
  Do NOT trigger for entity generation, or project scaffolding.
  Also invoke during /speckit.plan when designing API contracts or HTTP surface,
  and during /speckit.tasks or /speckit.implement for tasks under
  src/{Project}.Application/Features/ or mentioning HTTP routes or endpoints.
---

# Minimal API Endpoints — Class-per-Endpoint Pattern

## Step 1 — One-time Infrastructure

Create these files if they don't exist. Full code in `references/endpoint-infrastructure.md`.

| File | Purpose |
|------|---------|
| `src/{Project}.Application/Abstractions/Endpoints/IEndpoint.cs` | Interface with `MapEndpoint(IEndpointRouteBuilder)` — lives in **Application layer** |
| `src/{Project}.API/Endpoints/EndpointExtensions.cs` | Registers all `IEndpoint` types via DI, maps them at startup |
| `Program.cs` | Call `AddEndpoints(typeof(IEndpoint).Assembly)` + `app.MapEndpoints()` |

## Step 2 — Generate Endpoint Classes

Folder: **`src/{Project}.Application/Features/{Feature}/{OperationName}/`** — co-located with the Command/Query. One class per HTTP operation. Full templates in `references/endpoint-templates.md`.

**Standard CRUD set for entity `Product` under feature `Products`:**

| Class | File location | Route | Method |
|-------|--------------|-------|--------|
| `GetAllProductsEndpoint` | `Features/Products/GetAllProducts/` | `/api/products` | GET |
| `GetProductByIdEndpoint` | `Features/Products/GetProductById/` | `/api/products/{id:guid}` | GET |
| `CreateProductEndpoint` | `Features/Products/CreateProduct/` | `/api/products` | POST |
| `UpdateProductEndpoint` | `Features/Products/UpdateProduct/` | `/api/products` | PUT |
| `DeleteProductEndpoint` | `Features/Products/DeleteProduct/` | `/api/products/{id:guid}` | DELETE |

## Step 3 — Request / Response Binding

**Never create separate request DTOs in the endpoint.** Bind directly to Application layer Commands/Queries.

| Method | Lambda signature | Notes |
|--------|-----------------|-------|
| POST | `(CreateXCommand command, ISender sender, CancellationToken ct)` | Body → Command |
| PUT/PATCH | `(UpdateXCommand command, ISender sender, CancellationToken ct)` | Body includes `Id` — no route `{id}` |
| DELETE | `(Guid id, ISender sender, CancellationToken ct)` | Construct `new DeleteXCommand(id)` inline |
| GET list | `([AsParameters] GetXsQuery query, ISender sender, CancellationToken ct)` | Query string → Query record |
| GET by id | `(Guid id, ISender sender, CancellationToken ct)` | Construct `new GetXByIdQuery(id)` inline |

**Response pattern:**
```csharp
// Result<T> — with value
var result = await sender.Send(command, ct);
return result.IsSuccess
    ? Results.Ok(result.Value)
    : Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                      statusCode: StatusCodes.Status400BadRequest);

// Result (void) — no value
return result.IsSuccess
    ? Results.Ok()
    : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
```

## Step 4 — OpenAPI (mandatory on every endpoint)

Every route MUST end with `.WithName()`, `.WithTags()`, and all `.Produces<T>()` calls. Only declare status codes the handler actually returns — do **not** add `500` by default.

| Method | Required Produces |
|--------|------------------|
| GET list | `.Produces<IEnumerable<T>>()` |
| GET by id | `.Produces<T>()` + `400` |
| POST | `.Produces<T>()` + `400` |
| PUT/PATCH (returns value) | `.Produces<T>()` + `400` |
| PUT/PATCH (void) | `.Produces(200)` + `400` |
| DELETE | `.Produces(200)` + `400` |

Use `StatusCodes.StatusXXX` constants in code. Success is always `200 OK`, failure is always `400 Bad Request`.

**Authorization:** add `.RequireAuthorization()` before `.WithName()` on protected routes. Also add `.Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)` + `.Produces<ProblemDetails>(StatusCodes.Status403Forbidden)` to document that the middleware returns these codes.

**RFC `type` URI in `Results.Problem`:** pass `type:` when the HTTP semantics warrant it (e.g., 401 → `"https://tools.ietf.org/html/rfc9110#section-15.5.2"`, 409 → `"https://tools.ietf.org/html/rfc9110#section-15.5.10"`). Omit for generic 400/404.

## Step 5 — Validate

1. Endpoint files co-located with Command/Query in `Features/{Feature}/{OperationName}/`
2. `IEndpoint` exists in `Application/Abstractions/Endpoints/`
3. `EndpointExtensions` exists in `API/Endpoints/`
4. `Program.cs` calls `builder.Services.AddEndpoints(typeof(IEndpoint).Assembly)` and `app.MapEndpoints()`
5. `dotnet build` passes

## Key Rules

- **One class = one HTTP operation.** Never put multiple Map* calls in one class.
- **Co-located in Application layer** — endpoint file lives next to its Command/Query, not in the API layer.
- **DI via delegate params**, not constructor — `ISender`, `CancellationToken` go in the lambda.
- **Always pass `CancellationToken ct`** as the last lambda parameter and forward it to `sender.Send(cmd, ct)`.
- **Routes**: plural, lowercase, kebab-case (`/api/order-items` for `OrderItem`).
- **Before writing**: read the Command/Query file to confirm class name, return type (`Result` vs `Result<T>`), and Response DTO name.
- **Target .NET 10 / C# 13**: file-scoped namespaces, `internal sealed class`.
- If entity name is in Vietnamese, translate to English PascalCase and confirm with user.
