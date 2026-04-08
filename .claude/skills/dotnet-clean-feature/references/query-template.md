# Query Template Reference

## Query Record

Create `src/{ProjectName}.Application/Features/{EntityPlural}/{OperationName}/{OperationName}Query.cs`:

### Standard Query
```csharp
using {ProjectName}.Application.Abstractions.Messaging;

namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

public sealed record {OperationName}Query(
    // Parameters for filtering/identification
    // Example: Guid BookingId
) : IQuery<{ResponseType}>;
```

### Cached Query
```csharp
using {ProjectName}.Application.Abstractions.Caching;

namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

public sealed record {OperationName}Query(
    Guid {Entity}Id
) : ICachedQuery<{ResponseType}>
{
    public string CacheKey => $"{entityPlural}-{{{Entity}Id}}";
    public TimeSpan? Expiration => null; // or TimeSpan.FromMinutes(5)
}
```

**When to use ICachedQuery:**
- Data that doesn't change frequently (reference data, configuration)
- Expensive queries (complex joins, aggregations)
- High-traffic endpoints

**When to use plain IQuery:**
- Data that changes often
- Queries with user-specific authorization checks
- Simple, fast queries

---

## Query Handler

Create `src/{ProjectName}.Application/Features/{EntityPlural}/{OperationName}/{OperationName}QueryHandler.cs`:

### GetById Pattern
```csharp
using Dapper;
using {ProjectName}.Application.Abstractions.Data;
using {ProjectName}.Application.Abstractions.Messaging;
using {ProjectName}.Domain.Abstractions;

namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

internal sealed class {OperationName}QueryHandler(
    ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<{OperationName}Query, {ResponseType}>
{
    public async Task<Result<{ResponseType}>> Handle(
        {OperationName}Query request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                id          AS Id,
                name        AS Name,
                description AS Description,
                created_at  AS CreatedAt
            FROM {table_snake_case}
            WHERE id = @{Entity}Id
              AND is_deleted = false
            """;

        var result = await connection.QuerySingleOrDefaultAsync<{ResponseType}>(
            sql,
            new { request.{Entity}Id });

        return result is not null
            ? result
            : Result.Failure<{ResponseType}>(new Error("{Entity}.NotFound", "{Entity} không tồn tại."));
    }
}
```

### GetAll / Search Pattern
```csharp
internal sealed class {OperationName}QueryHandler(
    ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<{OperationName}Query, IReadOnlyList<{ResponseType}>>
{
    public async Task<Result<IReadOnlyList<{ResponseType}>>> Handle(
        {OperationName}Query request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                id          AS Id,
                name        AS Name,
                created_at  AS CreatedAt
            FROM {table_snake_case}
            WHERE is_deleted = false
            """;

        var results = await connection.QueryAsync<{ResponseType}>(sql);

        return results.ToList();
    }
}
```

### Query with Authorization (reads current user from IUserContext)
```csharp
internal sealed class {OperationName}QueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IUserContext userContext)
    : IQueryHandler<{OperationName}Query, {ResponseType}>
{
    public async Task<Result<{ResponseType}>> Handle(
        {OperationName}Query request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT ...
            FROM {table_snake_case}
            WHERE id = @UserId
              AND is_deleted = false
            """;

        var result = await connection.QuerySingleOrDefaultAsync<{ResponseType}>(
            sql,
            new { UserId = userContext.UserId });

        return result is not null
            ? result
            : Result.Failure<{ResponseType}>(new Error("{Entity}.NotFound", "{Entity} không tồn tại."));
    }
}
```

### Multi-mapping Pattern (joins)
When the response contains nested objects, use Dapper's multi-mapping:

```csharp
IEnumerable<{ResponseType}> results = await connection
    .QueryAsync<{ResponseType}, {NestedResponse}, {ResponseType}>(
        sql,
        (parent, child) =>
        {
            parent.{ChildProperty} = child;
            return parent;
        },
        new { /* parameters */ },
        splitOn: "{FirstColumnOfChildObject}");
```

---

## Response DTO

Create `src/{ProjectName}.Application/Features/{EntityPlural}/{OperationName}/{ResponseType}.cs` (same directory as the handler):

```csharp
namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

public sealed record {ResponseType}(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    DateTime CreatedAt);
```

**Guidelines:**
- `sealed record` with positional parameters — constructor parameter names must match SQL aliases exactly (Dapper maps by name)
- Always its own separate file — never inline inside the query file
- For nested objects, create separate response records (e.g., `AddressResponse`)
- If a DTO is shared across multiple features (e.g., `TokenResponse` used by Login and RefreshToken), place it in `Shared/Dtos/{ResponseType}.cs` instead

---

## Query Endpoint

Create `src/{ProjectName}.Application/Features/{EntityPlural}/{OperationName}/{OperationName}Endpoint.cs` (co-located with the query):

### GET — Single (returns 200 or 404)
```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using {ProjectName}.Application.Abstractions.Endpoints;

namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

internal sealed class {OperationName}Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/{entityPluralKebab}/{id}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new {OperationName}Query(id), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: StatusCodes.Status404NotFound);
        })
        .RequireAuthorization()
        .WithName("{OperationName}")
        .WithTags("{EntityPlural}")
        .Produces<{ResponseType}>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
```

### GET — List (returns 200)
```csharp
internal sealed class {OperationName}Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/{entityPluralKebab}", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new {OperationName}Query(), ct);

            return Results.Ok(result.Value);
        })
        .RequireAuthorization()
        .WithName("{OperationName}")
        .WithTags("{EntityPlural}")
        .Produces<IReadOnlyList<{ResponseType}>>();
    }
}
```

### GET — Auth-scoped (reads current user, no route param)
```csharp
internal sealed class {OperationName}Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/{entityPluralKebab}/me", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new {OperationName}Query(), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: StatusCodes.Status404NotFound);
        })
        .RequireAuthorization()
        .WithName("{OperationName}")
        .WithTags("{EntityPlural}")
        .Produces<{ResponseType}>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
```

**Guidelines:**
- Class is `internal sealed` — same as handler
- Route: `/api/{entityPluralKebab}` (kebab-case plural, e.g., `/api/products`)
- `EndpointExtensions` scans the Application assembly and registers all `IEndpoint` implementations — no manual wiring needed
- Use `.RequireAuthorization()` for protected endpoints
- For auth-scoped queries (reads from `IUserContext`), the endpoint sends an empty query — no user ID in the route

