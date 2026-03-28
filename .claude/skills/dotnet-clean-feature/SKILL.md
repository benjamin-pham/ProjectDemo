---
name: dotnet-clean-feature
description: >
  Generate MediatR CQRS features — individual Commands or Queries with Handlers, Validators,
  and Response DTOs — for an ASP.NET Core Clean Architecture project following
  Pragmatic Clean Architecture patterns. Uses vertical slice folder structure,
  custom Result pattern, FluentValidation, Dapper for queries (CQRS read side),
  and EF Core repositories for commands (write side).
  Use this skill whenever the user wants to create/add/generate a command, query,
  or handler in a .NET clean architecture project.
  Trigger on: "tạo feature", "thêm command", "add query", "thêm use case",
  "create feature for Product", "sinh command CreateProduct", "add GetAll query", "tạo handler",
  "tạo CRUD cho entity X", "tạo API cho entity X" — or any Vietnamese/English request about
  adding MediatR commands, queries, handlers, validators, or application-layer features.
  Also invoke during /speckit.plan when designing Application layer use cases,
  and during /speckit.tasks or /speckit.implement for tasks under
  src/{Project}.Application/Features/ or mentioning commands, queries, handlers, or validators.
---

# ASP.NET Core — Clean Architecture Feature Generator (MediatR CQRS)

## Prerequisites
- `Application/Abstractions/Messaging/` — `ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler`
- `Domain/Abstractions/` — `Result<T>`, `Error`, `IUnitOfWork`
- `Application/Abstractions/Data/` — `ISqlConnectionFactory`
- `ValidationBehavior` pipeline registered in `DependencyInjection.cs`

## Project Detection

Find `.sln`/`.slnx` to identify `{ProjectName}`. Application project is at `src/{ProjectName}.Application/`.

---

## Folder Structure — Vertical Slice

```
src/{ProjectName}.Application/
  Features/
    {EntityPlural}/                     # e.g., Products/
      {OperationName}/                  # e.g., CreateProduct/
        README.md
        {OperationName}Command.cs       # or Query.cs
        {OperationName}CommandHandler.cs
        {OperationName}CommandValidator.cs   # Commands only
        {ResponseType}.cs               # if operation returns a DTO
      Shared/                           # Only if 2+ operations share a validator
        {SharedValidator}.cs
  Shared/
    Dtos/                               # DTOs shared across entity groups
```

**Naming:** Use domain language — `ReserveBooking` over `CreateBooking`, `RegisterUser` over `CreateUser`. When domain language isn't obvious, generic CRUD is fine.

---

## Generating a Command

Read `references/command-template.md` for full code templates. Generate these files:

1. **`{OperationName}Command.cs`** — `public sealed record` implementing `ICommand<TResponse>`
   - Properties are primitive types only — no domain objects as parameters
   - Use `ICommand` (no generic) for void, `ICommand<Guid>` for create returning ID

2. **`{OperationName}CommandHandler.cs`** — `internal sealed class` implementing `ICommandHandler<,>`
   - Primary constructor injection
   - Load entity → call domain method → `unitOfWork.SaveChangesAsync(cancellationToken)`
   - Return `Result.Failure<T>(new Error(...))` for business errors — never throw
   - For current user: inject `IUserContext`, don't add `UserId` to the Command record

3. **`{OperationName}CommandValidator.cs`** — `internal class` (not sealed) extending `AbstractValidator<>`
   - Validate shape/format only — business rules belong in the Handler
   - Every required property needs at least `NotEmpty()`; strings need `MaximumLength()` matching DB constraint

4. **`{ResponseType}.cs`** — `public sealed record` (only if command returns a DTO, not `Result` or `Result<Guid>`)

---

## Generating a Query

Read `references/query-template.md` for full code templates. Generate these files:

1. **`{OperationName}Query.cs`** — `public sealed record` implementing `IQuery<TResponse>`
   - For cacheable data: implement `ICachedQuery<TResponse>` with `CacheKey` and `Expiration`

2. **`{OperationName}QueryHandler.cs`** — `internal sealed class` implementing `IQueryHandler<,>`
   - Inject `ISqlConnectionFactory` (and `IUserContext` if authorization needed)
   - Use **Dapper + raw SQL only** — never EF Core or repositories in query handlers
   - SQL: snake_case column names with PascalCase aliases (`created_at AS CreatedAt`), raw string literals (`"""..."""`), parameterized queries

3. **`{ResponseType}.cs`** — `public sealed record` with positional parameters matching SQL aliases exactly (Dapper maps by name)

---

## README.md

Every operation folder **must** have a `README.md`. Read `references/readme-template.md` for the template.
- New operation: generate README with Description, Input, Output, Validation Rules, Version History
- Existing operation: update changed sections and append a new row to Version History

---

## Key Rules (see `references/conventions.md` for full list)

| Rule | Value |
|------|-------|
| Namespaces | File-scoped (`namespace X;`) |
| Handlers | `internal sealed class` |
| Validators | `internal class` (not sealed — may be extended) |
| Commands/Queries/DTOs | `public sealed record` |
| Write side | Repository + `IUnitOfWork.SaveChangesAsync()` |
| Read side | Dapper + `ISqlConnectionFactory` — never EF Core |
| CancellationToken | Every `Handle` method and every async call inside it |
| Vietnamese input | Translate operation names to English PascalCase, confirm with user |
