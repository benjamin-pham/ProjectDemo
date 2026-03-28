---
name: dotnet-clean-entity
description: >
  Generate rich domain entities for ASP.NET Core Clean Architecture projects.
  Produces entities with a static factory method (Create) following DDD Rich
  Domain Model patterns. Domain layer only — no EF config, repositories, or DI.
  Use whenever the user wants to create/add a new entity, model, aggregate, or
  domain object in a .NET project. Trigger on: "tạo entity", "thêm entity",
  "add entity", "create entity Product", "tạo model Order", "tạo aggregate",
  "thêm domain object", or any request about domain entities with factory
  methods or encapsulation. Even if user just says "tạo entity X", use this
  skill — all entities follow rich domain model by default.
  Also invoke during /speckit.plan when authoring data-model.md,
  and during /speckit.tasks or /speckit.implement for tasks under
  src/{Project}.Domain/Entities/ or mentioning entities.
---

# Rich Domain Entity Generator

Domain layer only. No EF configs, repositories, or DI wiring.

## Steps

1. Find `.slnx`/`.sln` to identify `{ProjectName}`.
2. Ensure `BaseEntity.cs` exists in `src/{ProjectName}.Domain/Abstractions/` — create if missing.
3. Infer sensible properties from entity name if user doesn't specify (e.g., `Product` → Name, Description, Price).
4. Create entity at `src/{ProjectName}.Domain/Entities/{EntityName}.cs`.
5. If entity has an enum, create `src/{ProjectName}.Domain/Enums/{EnumName}.cs` separately.
6. Vietnamese entity names → translate to English PascalCase, confirm with user.

---

## BaseEntity.cs

`src/{ProjectName}.Domain/Abstractions/BaseEntity.cs`:

```csharp
namespace {ProjectName}.Domain.Abstractions;

public abstract class BaseEntity<TKey> where TKey : notnull
{
    public TKey Id { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public BaseEntity() { CreatedAt = DateTime.UtcNow; }
    public void SoftDelete() { IsDeleted = true; }
}

public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity() { Id = Guid.NewGuid(); }
}
```

---

## Entity Template

`src/{ProjectName}.Domain/Entities/{EntityName}.cs`:

```csharp
using {ProjectName}.Domain.Abstractions;
using {ProjectName}.Domain.Enums;

namespace {ProjectName}.Domain.Entities;

public class {EntityName} : BaseEntity
{
    // Scalar props — public setter required by EF Core
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }

    // Scalar navigation (no setter — EF loads it)
    public Category? Category { get; }

    // Collection navigation — private backing field enforces encapsulation
    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Parameterless ctor required by EF Core
    public {EntityName}() { }

    // Factory method — the ONLY way to create a valid instance
    public static {EntityName} Create(
        string name,
        decimal price,
        Guid categoryId,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new {EntityName}
        {
            Name = name,
            Price = price,
            CategoryId = categoryId,
            Description = description,
            Status = {EntityName}Status.Pending   // enum default set here, not as param
        };
    }

    // State-transition behavior method
    public void Confirm()
    {
        if (Status != {EntityName}Status.Pending)
            throw new InvalidOperationException($"Cannot confirm in '{Status}' status.");
        Status = {EntityName}Status.Confirmed;
    }

    // Collection mutation behavior method
    public void AddItem(OrderItem item) => _items.Add(item);
}
```

---

## Rules

- **`Create()` params**: required first (no defaults) → optional last (`= null`). Enum default state set inside body, not as parameter.
- **Guard clauses**: `ArgumentException.ThrowIfNullOrWhiteSpace()` for required strings.
- **`= default!`** on required non-nullable props (EF Core needs public setters to populate them).
- **Collection nav**: always use private `List<T>` backing field + public `IReadOnlyCollection<T>`. Expose mutation via behavior methods only.
- **File-scoped namespaces**. C# 13 collection expressions `[]`. No primary constructors on entities.
- Navigation FK props belong in the entity; EF relationship config goes to the infrastructure layer.
