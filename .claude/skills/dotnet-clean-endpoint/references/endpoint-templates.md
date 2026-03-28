# Endpoint Templates

Replace `Product`/`Products` with the actual entity name. Commands and Queries come from the Application layer.

## Folder Structure

```
src/{ProjectName}.API/Endpoints/
├── IEndpoint.cs
├── EndpointExtensions.cs
└── Products/
    ├── GetAllProductsEndpoint.cs
    ├── GetProductByIdEndpoint.cs
    ├── CreateProductEndpoint.cs
    ├── UpdateProductEndpoint.cs
    └── DeleteProductEndpoint.cs
```

## GetAll

```csharp
using MediatR;
using {ProjectName}.Application.Features.Products.GetAllProducts;

namespace {ProjectName}.API.Endpoints.Products;

internal sealed class GetAllProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async ([AsParameters] GetAllProductsQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            return Results.Ok(result.Value);
        })
        .WithName("GetAllProducts")
        .WithTags("Products")
        .Produces<IEnumerable<ProductResponse>>()
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
```

## GetById

```csharp
using MediatR;
using {ProjectName}.Application.Features.Products.GetProductById;

namespace {ProjectName}.API.Endpoints.Products;

internal sealed class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                                  statusCode: StatusCodes.Status404NotFound);
        })
        .WithName("GetProductById")
        .WithTags("Products")
        .Produces<ProductResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
```

## Create

```csharp
using MediatR;
using {ProjectName}.Application.Features.Products.CreateProduct;

namespace {ProjectName}.API.Endpoints.Products;

internal sealed class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", async (CreateProductCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/products/{result.Value.Id}", result.Value)
                : Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                                  statusCode: StatusCodes.Status400BadRequest);
        })
        .WithName("CreateProduct")
        .WithTags("Products")
        .Produces<CreateProductResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
```

## Update

```csharp
using MediatR;
using {ProjectName}.Application.Features.Products.UpdateProduct;

namespace {ProjectName}.API.Endpoints.Products;

internal sealed class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/products", async (UpdateProductCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error.Code.EndsWith("NotFound")
                    ? Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                                      statusCode: StatusCodes.Status404NotFound)
                    : Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                                      statusCode: StatusCodes.Status400BadRequest);
        })
        .WithName("UpdateProduct")
        .WithTags("Products")
        .Produces<UpdateProductResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
```

> If `UpdateProductCommand` returns `Result` (void), use `Results.NoContent()` + `.Produces(204)` instead.

## Delete

```csharp
using MediatR;
using {ProjectName}.Application.Features.Products.DeleteProduct;

namespace {ProjectName}.API.Endpoints.Products;

internal sealed class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/products/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(title: result.Error.Code, detail: result.Error.Description,
                                  statusCode: StatusCodes.Status404NotFound);
        })
        .WithName("DeleteProduct")
        .WithTags("Products")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
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

## Result (void) — No Response DTO

When handler returns `Result` (not `Result<T>`):

```csharp
var result = await sender.Send(command);
return result.IsSuccess
    ? Results.NoContent()
    : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
```

Use `.Produces(StatusCodes.Status204NoContent)` instead of `.Produces<T>()`.
