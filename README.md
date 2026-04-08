# Commands

```bash
# Build
dotnet build

# Run API (target .NET 10)
dotnet run --project src/MyProject.API/MyProject.API.csproj

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/MyProject.Application.UnitTests/

# Run a specific test
dotnet test --filter "FullyQualifiedName~SomeTestName"
```

No solution file — this is a modern .NET 10 project using directory-level build props.

# Architecture

Clean Architecture with 4 layers (strict unidirectional dependency: API → Application → Domain; Infrastructure → Domain):

```
        ┌─────────────────────────────────────────────────────┐
        │                     MyProject.API                   │
        │            (Endpoints, Middleware, DI wiring)       │
        └───────────────┬─────────────────┬───────────────────┘
                        │ depends on      │ registers (DI only)
                        ▼                 ▼
        ┌─────────────────────────┐  ┌─────────────────────────────┐
        │  MyProject.Application  │  │   MyProject.Infrastructure  │
        │ (Commands, Queries,     │  │  (EF Core, Repositories,    │
        │  Handlers,Validators)   │  │   DbContext)                │
        └───────────┬─────────────┘  └───────────────┬─────────────┘
                    │ depends on                     │ depends on
                    ▼                                ▼
            ┌──────────────────────────────────────────────┐
            │                MyProject.Domain              │
            │    (Entities, Abstractions, Enums, Result)   │
            └──────────────────────────────────────────────┘
```


```
src/
├── {ProjectName}.Domain/
│   ├── Abstractions/          ← IRepository<T>, IUnitOfWork, IUserContext, IDateTimeProvider, Result<T>, Error
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
│               ├── {OperationName}CommandValidator.cs    ← Commands only
│               ├── {OperationName}Response.cs            ← if operation returns a DTO — always its own file, same directory as handler
│               └── README.md                             ← Business documentation
│
├── {ProjectName}.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/    ← OrderConfiguration : IEntityTypeConfiguration<Order>
│   └── Repositories/          ← OrderRepository : IOrderRepository
│
└── {ProjectName}.API/
    ├── Endpoints/             ← EndpointExtensions ← Auto register endpoint
    └── Extensions/            ← GlobalExceptionHandler, CorrelationIdMiddleware, SerilogExtensions
```

# Key Patterns

**Endpoint registration** — Implement `IEndpoint` (defined in `src/MyProject.Application/Abstractions/Endpoints/IEndpoint.cs`), place the endpoint file **co-located with its Command/Query** in `src/MyProject.Application/Features/{Feature}/{OperationName}/`. `EndpointExtensions` scans the assembly at startup and registers all `IEndpoint` implementations automatically — no manual wiring needed.

**Commands/Queries** — Each operation lives in its own folder `src/MyProject.Application/Features/{Feature}/{OperationName}/` and contains:

| File | Required | Notes |
|---|---|---|
| `{OperationName}Command.cs` / `{OperationName}Query.cs` | yes | MediatR `IRequest<Result<T>>` — record type |
| `{OperationName}CommandHandler.cs` / `{OperationName}QueryHandler.cs` | yes | Implements `ICommandHandler<,>` / `IQueryHandler<,>` |
| `{OperationName}CommandValidator.cs` / `{OperationName}QueryValidator.cs` | when validation needed | `AbstractValidator<TCommand>` / `AbstractValidator<TQuery>`; pipeline picks it up automatically |
| `{OperationName}Endpoint.cs` | yes | `IEndpoint` implementation co-located here (see Endpoint registration above) |
| `{OperationName}Response.cs` | if returns DTO | Response record, same folder |
| `README.md` | yes | Business documentation — what this operation does, rules, edge cases |

**Result pattern** — Domain errors use `Result<T>` (not exceptions). Use `Result.Success(value)` / `Result.Failure(error)` and check `result.IsFailure` in handlers or endpoints.

**Audit trail** — All entities extending `BaseEntity` automatically get `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` set by `AppDbContext.SaveChangesAsync`. `IsDeleted` enables soft deletes.

**EF Core config** — Entity configurations go in `src/MyProject.Infrastructure/Data/Configurations/` using Fluent API with snake_case naming convention.

# Testing

| Project | Scope | Key Dependencies |
|---|---|---|
| `Domain.UnitTests` | Entity logic | xunit, FluentAssertions |
| `Application.UnitTests` | Handlers, validators, behaviors | + NSubstitute |
| `Infrastructure.IntegrationTests` | Repositories, EF Core config | + Testcontainers (PostgreSQL), Respawn |
| `API.IntegrationTests` | End-to-end HTTP | + WebApplicationFactory, Testcontainers, Respawn |
| `ArchitectureTests` | Layer dependency enforcement | NetArchTest.Rules |

Integration tests spin up a real PostgreSQL container via Testcontainers. Respawn resets data between tests.

# Package Management

All NuGet versions are centrally managed in `Directory.Packages.props`. Do not set `Version` on `<PackageReference>` in individual project files; use `VersionOverride` only when necessary.

# Code Style

- Nullable reference types enabled
- Async all the way - no .Result or .Wait()
- Record types for DTOs
- Always IOptions<T> or IOption no raw config["Key]
- NEVER use DateTime.Now - use IDateTimeProvider