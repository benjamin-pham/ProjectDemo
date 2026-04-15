# Commands

```bash
# Build
dotnet build

# Run WebHost (target .NET 10)
dotnet run --project src/{ProjectName}.WebHost/{ProjectName}.WebHost.csproj

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/{ProjectName}.Application.UnitTests/

# Run a specific test
dotnet test --filter "FullyQualifiedName~SomeTestName"
```

This is a modern .NET 10 project using directory-level build props.

# Architecture

Clean Architecture with 4 layers (strict unidirectional dependency: WebHost → Application → Domain; Infrastructure → Domain):

```mermaid
graph TD
    WebHost["<b>WebHost</b><br/>({ProjectName}.WebHost)<br/>Entrypoint"]
    APP["<b>Application</b><br/>({ProjectName}.Application)<br/>Commands, Queries, Handlers, Validators, Endpoints"]
    INF["<b>Infrastructure</b><br/>({ProjectName}.Infrastructure)<br/>EF Core, Repositories, DbContext"]
    DOM["<b>Domain</b><br/>({ProjectName}.Domain)<br/>Entities, Abstractions, Enums"]

    WebHost -->|depends on| APP
    WebHost -->|registers| INF
    APP -->|depends on| DOM
    INF -->|depends on| DOM

    style DOM fill:#4CAF50,color:#fff,stroke:#388E3C
    style APP fill:#2196F3,color:#fff,stroke:#1565C0
    style INF fill:#FF9800,color:#fff,stroke:#E65100
    style WebHost fill:#9C27B0,color:#fff,stroke:#6A1B9A
```

```
src/
├── {ProjectName}.Domain/
│   ├── Abstractions/          ← IRepository<T>, IUnitOfWork, IUserContext, IDateTimeProvider, Result<T>, Error, PagedListFilter, PagedList<T>
│   ├── Entities/              ← Order, Customer, Product (with behavior methods)
│   ├── Enums/                 ← OrderStatus, PaymentMethod
│   └── Repositories/          ← IOrderRepository, IProductRepository (entity-specific interfaces)
│
├── {ProjectName}.Application/
│   ├── Abstractions/
│   │   ├── Data/              ← ISqlConnectionFactory
│   │   ├── Messaging/         ← ICommand, IQuery, ICommandHandler, IQueryHandler
│   │   ├── Endpoints/         ← IEndpoint
│   │   └── {Feature}/         ← feature-specific interfaces (e.g., Authentication/IJwtTokenService)
│   ├── Behaviors/             ← ValidationBehavior
│   ├── Exceptions/            ← ValidationException, ValidationError
│   ├── Shared/                ← Common
│   │   └── Dtos/              ← Reusable DTOs shared across operations in this project
│   │   └── RuleValidator/     ← Reusable validators shared across operations in this project
│   └── Features/
│       └── {EntityPlural}/        ← Feature folder per aggregatee.g., Users/, Orders/, Products/
│           ├── Shared/            ← Reusable validators shared across operations in this group
│           └── {OperationName}/   ← e.g., Register/
│               ├── {OperationName}Endpoint.cs            ← handler api
│               ├── {OperationName}Command.cs             ← or {OperationName}Query.cs
│               ├── {OperationName}CommandHandler.cs      ← or {OperationName}QueryHandler.cs
│               ├── {OperationName}CommandValidator.cs    ← or {OperationName}QueryValidator.cs
│               ├── {OperationName}Response.cs            ← if operation returns a DTO — always its own file, same directory as handler
│               └── README.md                             ← Business documentation
│
├── {ProjectName}.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/    ← OrderConfiguration : IEntityTypeConfiguration<Order>
│   └── Repositories/          ← OrderRepository : IOrderRepository
│
└── {ProjectName}.WebHost/     ← Entrypoint
```

# Key Patterns

**Endpoint registration** — Implement `IEndpoint` (defined in `src/{ProjectName}.Application/Abstractions/Endpoints/IEndpoint.cs`), place the endpoint file **co-located with its Command/Query** in `src/{ProjectName}.Application/Features/{Feature}/{OperationName}/`. `EndpointExtensions` scans the assembly at startup and registers all `IEndpoint` implementations automatically — no manual wiring needed.

**Commands/Queries** — Each operation lives in its own folder `src/{ProjectName}.Application/Features/{Feature}/{OperationName}/` and contains:

| File | Required | Notes |
|---|---|---|
| `{OperationName}Command.cs` / `{OperationName}Query.cs` | yes | MediatR `IRequest<Result<T>>` — record type |
| `{OperationName}CommandHandler.cs` / `{OperationName}QueryHandler.cs` | yes | Implements `ICommandHandler<,>` / `IQueryHandler<,>` |
| `{OperationName}CommandValidator.cs` / `{OperationName}QueryValidator.cs` | when validation needed | `AbstractValidator<TCommand>` / `AbstractValidator<TQuery>`; for paginated queries, validator should inherit `PagedListValidator<TQuery>`; pipeline picks it up automatically |
| `{OperationName}Endpoint.cs` | yes | `IEndpoint` implementation co-located here (see Endpoint registration above) |
| `{OperationName}Response.cs` | if returns DTO | Response record, same folder |
| `README.md` | yes | Business documentation — what this operation does, rules, edge cases |

**Result pattern** — Domain errors use `Result<T>` (not exceptions). Use `Result.Success(value)` / `Result.Failure(error)` and check `result.IsFailure` in handlers or endpoints.

**Pagination** — When an operation needs paging, let the query inherit `PagedListFilter` from `src/{ProjectName}.Domain/Abstractions/PagedListFilter.cs` so it carries `PageNumber`, `PageSize`, `SortBy`, `SortDirection`, and `SearchTerm`. The corresponding query validator should inherit `PagedListValidator<TQuery>` from `src/{ProjectName}.Application/Shared/RuleValidator/PagedListValidator.cs` to validate `PageNumber` and `PageSize`. Return `PagedList<T>` from `src/{ProjectName}.Domain/Abstractions/PagedList.cs` so responses consistently include `Items`, `PageNumber`, `PageSize`, `TotalItems`, `TotalPages`, `HasNextPage`, and `HasPreviousPage`. For paginated read queries, use Dapper `QueryMultipleAsync` to fetch both the page items and total count in a single database round-trip.

**Audit trail** — All entities extending `BaseEntity` automatically get `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` set by `AppDbContext.SaveChangesAsync`. `IsDeleted` enables soft deletes.

**EF Core config** — Entity configurations go in `src/{ProjectName}.Infrastructure/Data/Configurations/` using Fluent API with snake_case naming convention.

# Testing

| Project | Scope | Key Dependencies |
|---|---|---|
| `Domain.UnitTests` | Entity logic | xunit, FluentAssertions |
| `Application.UnitTests` | Handlers, validators, behaviors | + NSubstitute |
| `Application.IntegrationTests` | End-to-end HTTP | + WebApplicationFactory, Testcontainers, Respawn |
| `Infrastructure.IntegrationTests` | Repositories, EF Core config | + Testcontainers (PostgreSQL), Respawn |
| `ArchitectureTests` | Layer dependency enforcement | NetArchTest.Rules |

Integration tests spin up a real PostgreSQL container via Testcontainers. Respawn resets data between tests.

# Package Management

All NuGet versions are centrally managed in `Directory.Packages.props`. Do not set `Version` on `<PackageReference>` in individual project files; use `VersionOverride` only when necessary.

# Code Style

- Nullable reference types enabled
- Async all the way - no .Result or .Wait()
- Record types for DTOs
- Always IOptions<T> no raw config["Key"]
- NEVER use DateTime.Now - use IDateTimeProvider
