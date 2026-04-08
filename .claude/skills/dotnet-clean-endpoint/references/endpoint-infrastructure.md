# Endpoint Infrastructure (One-Time Setup)

## IEndpoint Interface

`src/{ProjectName}.Application/Abstractions/Endpoints/IEndpoint.cs`

> Lives in the **Application layer** so endpoint classes co-located with Commands/Queries can reference it without a circular dependency.

```csharp
using Microsoft.AspNetCore.Routing;

namespace {ProjectName}.Application.Abstractions.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
```

## EndpointExtensions — DI-based Registration

`src/{ProjectName}.API/Endpoints/EndpointExtensions.cs`

```csharp
using System.Reflection;
using {ProjectName}.Application.Abstractions.Endpoints;

namespace {ProjectName}.API.Endpoints;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly)
    {
        var endpointTypes = assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && t.IsAssignableTo(typeof(IEndpoint)));

        foreach (var type in endpointTypes)
            services.AddTransient(typeof(IEndpoint), type);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (var endpoint in endpoints)
            endpoint.MapEndpoint(builder);

        return app;
    }
}
```

**How it works:**
- `AddEndpoints(assembly)` scans the given assembly, finds every concrete `IEndpoint`, and registers each as `IEnumerable<IEndpoint>` in DI.
- `MapEndpoints()` resolves the collection from DI and calls `MapEndpoint` on each — no manual wiring needed.
- Pass `typeof(IEndpoint).Assembly` to scan the **Application** assembly, where endpoint classes live.

## Program.cs Update

```csharp
using {ProjectName}.API.Endpoints;
using {ProjectName}.Application.Abstractions.Endpoints;

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpoints(typeof(IEndpoint).Assembly); // scans Application assembly
builder.Services.AddApplicationServices();
// ... other services

// ── Pipeline ──────────────────────────────────────────────────────────────────
app.MapEndpoints();
app.Run();
```

Key points:
- `typeof(IEndpoint).Assembly` resolves to the **Application** assembly — this is where all endpoint classes live.
- Two-step: register types in DI (`AddEndpoints`) then map routes (`MapEndpoints`).
