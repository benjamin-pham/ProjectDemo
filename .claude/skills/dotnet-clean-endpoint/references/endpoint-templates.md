# Endpoint Templates

Replace `Product`/`Products` with the actual entity name. Commands and Queries come from the Application layer.

## Folder Structure

Endpoint classes live **co-located with their Command/Query** in the Application layer:

```
src/{ProjectName}.Application/Features/
└── Products/
    ├── GetAllProducts/
    │   ├── GetAllProductsQuery.cs
    │   ├── GetAllProductsQueryHandler.cs
    │   ├── GetAllProductsEndpoint.cs      ← endpoint here
    │   ├── ProductSummaryResponse.cs
    │   └── README.md
    ├── GetProductById/
    │   ├── GetProductByIdQuery.cs
    │   ├── GetProductByIdQueryHandler.cs
    │   ├── GetProductByIdEndpoint.cs
    │   ├── ProductResponse.cs
    │   └── README.md
    ├── CreateProduct/
    │   ├── CreateProductCommand.cs
    │   ├── CreateProductCommandHandler.cs
    │   ├── CreateProductCommandValidator.cs
    │   ├── CreateProductEndpoint.cs
    │   ├── CreateProductResponse.cs
    │   └── README.md
    ├── UpdateProduct/
    │   ├── UpdateProductCommand.cs
    │   ├── UpdateProductCommandHandler.cs
    │   ├── UpdateProductCommandValidator.cs
    │   ├── UpdateProductEndpoint.cs
    │   ├── UpdateProductResponse.cs       ← only if command returns Result<T>
    │   └── README.md
    └── DeleteProduct/
        ├── DeleteProductCommand.cs
        ├── DeleteProductCommandHandler.cs
        ├── DeleteProductEndpoint.cs
        └── README.md
```

## GetAll

```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace {ProjectName}.Application.Features.Products.GetAllProducts;

internal sealed class GetAllProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async ([AsParameters] GetAllProductsQuery query, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(query, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                                  statusCode: StatusCodes.Status400BadRequest);
        })
        .WithName("GetAllProducts")
        .WithTags("Products")
        .Produces<IEnumerable<ProductSummaryResponse>>();
    }
}
```

## GetById

```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace {ProjectName}.Application.Features.Products.GetProductById;

internal sealed class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                                  statusCode: StatusCodes.Status404NotFound);
        })
        .WithName("GetProductById")
        .WithTags("Products")
        .Produces<ProductResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
```

## Create

```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace {ProjectName}.Application.Features.Products.CreateProduct;

internal sealed class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", async (CreateProductCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/products/{result.Value.Id}", result.Value)
                : Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                                  statusCode: StatusCodes.Status400BadRequest);
        })
        .WithName("CreateProduct")
        .WithTags("Products")
        .Produces<CreateProductResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
```

## Update

```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace {ProjectName}.Application.Features.Products.UpdateProduct;

internal sealed class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/products", async (UpdateProductCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                                  statusCode: StatusCodes.Status404NotFound);
        })
        .WithName("UpdateProduct")
        .WithTags("Products")
        .Produces<UpdateProductResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
```

> If `UpdateProductCommand` returns `Result` (void), use `Results.NoContent()` + `.Produces(204)` instead.

## Delete

```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace {ProjectName}.Application.Features.Products.DeleteProduct;

internal sealed class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/products/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                                  statusCode: StatusCodes.Status404NotFound);
        })
        .WithName("DeleteProduct")
        .WithTags("Products")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
```

## Route Naming Convention

| Entity | Route | Tag |
|--------|-------|-----|
| `Product` | `/api/products` | Products |
| `Category` | `/api/categories` | Categories |
| `OrderItem` | `/api/order-items` | Order Items |
| `UserProfile` | `/api/user-profiles` | User Profiles |

## Authorization

Add `.RequireAuthorization()` before `.WithName()` on any protected route:

```csharp
app.MapGet("/api/auth/me", async (...) => { ... })
    .RequireAuthorization()
    .WithName("GetProfile")
    .WithTags("Auth")
    ...
```

## RFC `type` URI in `Results.Problem`

Pass `type:` only when the status code has a well-known RFC meaning:

| Status | `type` URI |
|--------|-----------|
| 401 Unauthorized | `"https://tools.ietf.org/html/rfc9110#section-15.5.2"` |
| 409 Conflict | `"https://tools.ietf.org/html/rfc9110#section-15.5.10"` |

For generic 400 / 404 responses, omit `type:`.

```csharp
// With type (401)
Results.Problem(
    title: result.Error.Code,
    detail: result.Error.Description,
    statusCode: StatusCodes.Status401Unauthorized,
    type: "https://tools.ietf.org/html/rfc9110#section-15.5.2")

// Without type (400/404)
Results.Problem(
    title: result.Error.Code,
    detail: result.Error.Description,
    statusCode: StatusCodes.Status400BadRequest)
```

## Result (void) — No Response DTO

When handler returns `Result` (not `Result<T>`):

```csharp
var result = await sender.Send(command, ct);
return result.IsSuccess
    ? Results.NoContent()
    : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
```

Use `.Produces(StatusCodes.Status204NoContent)` instead of `.Produces<T>()`.
