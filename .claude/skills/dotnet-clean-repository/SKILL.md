---
name: dotnet-clean-repository
description: >
  Generate EF Core infrastructure for entities in an ASP.NET Core Clean
  Architecture project. Produces: Fluent API configuration with snake_case
  naming, repository interface + implementation, DbSet registration, and
  DI wiring. Use whenever the user wants to add EF Core config,
  repository, or persistence layer for a domain entity. Trigger on: "tạo
  repository", "thêm repository", "add repository", "tạo EF config",
  "tạo EF Core configuration", "tạo IRepository interface", "add DbSet",
  "thêm DbSet cho entity", "setup persistence", "cấu hình EF Core",
  "EF Core entity mapping", "Fluent API mapping",
  or any Vietnamese/English request about EF Core configuration, repository
  pattern, or database mapping in a .NET clean architecture project.
  Do NOT trigger on "tạo configuration" alone (too generic — could mean AppSettings
  or other config); EF/database/persistence context must be present.
  Also invoke during /speckit.plan when designing the persistence layer,
  and during /speckit.tasks or /speckit.implement for tasks under
  src/{Project}.Infrastructure/ or mentioning EF config, repositories, or DbSet.
---

# EF Core & Repository Generator

Generates infrastructure layer for a domain entity: EF config, repository interface/impl, DbSet, DI.

**Before generating:** check that `IRepository<T>`, `Repository<T>`, `BaseEntityConfiguration<T>`, and `AppDbContext` already exist. Create them first if missing.

---

## 1 — EF Core Configuration

`src/{ProjectName}.Infrastructure/Data/Configurations/{EntityName}Configuration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {ProjectName}.Domain.Abstractions;
using {ProjectName}.Domain.Entities;

namespace {ProjectName}.Infrastructure.Data.Configurations;

public class {EntityName}Configuration : BaseEntityConfiguration<{EntityName}>
{
    public override void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        base.Configure(builder);  // maps BaseEntity fields — always call first

        builder.ToTable("{snake_case_plural}");  // Product → products, OrderItem → order_items

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);  // enum → string

        // FK: builder.HasOne(x => x.Category).WithMany(x => x.Products)
        //         .HasForeignKey(x => x.CategoryId).HasConstraintName("fk_{table}_{column}");
        // Index: builder.HasIndex(x => x.Name).HasDatabaseName("ix_{table}_name");
    }
}
```

**Naming rules (snake_case):**
- Table: plural — `Product` → `products`, `OrderItem` → `order_items`, `Category` → `categories`
- Column: `CategoryId` → `category_id`, `CreatedAt` → `created_at`
- FK constraint: `fk_{table}_{column}` — e.g. `fk_products_category_id`
- Index: `ix_{table}_{columns}` — e.g. `ix_products_name`

See `references/snake_case_rules.md` for full pluralization rules and acronym handling.

---

## 2 — Repository Interface (Domain Layer)

`src/{ProjectName}.Domain/Repositories/I{EntityName}Repository.cs`

```csharp
using {ProjectName}.Domain.Abstractions;
using {ProjectName}.Domain.Entities;

namespace {ProjectName}.Domain.Repositories;

public interface I{EntityName}Repository : IRepository<{EntityName}>
{
    // Add entity-specific query methods here if needed
    // Task<IReadOnlyList<{EntityName}>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
}

// Non-Guid key (e.g., int): IRepository<{EntityName}, int>
```

---

## 3 — Repository Implementation

`src/{ProjectName}.Infrastructure/Repositories/{EntityName}Repository.cs`

```csharp
using {ProjectName}.Domain.Abstractions;
using {ProjectName}.Domain.Entities;
using {ProjectName}.Domain.Repositories;
using {ProjectName}.Infrastructure.Data;

namespace {ProjectName}.Infrastructure.Repositories;

public class {EntityName}Repository(AppDbContext context)
    : Repository<{EntityName}>(context), I{EntityName}Repository
{
    // Implement entity-specific query methods here
}

// Non-Guid key (e.g., int): Repository<{EntityName}, int>(context)
```

---

## 4 — DbSet in AppDbContext

```csharp
public DbSet<{EntityName}> {EntityNamePlural} => Set<{EntityName}>();
```

---

## 5 — DI Registration

`src/{ProjectName}.Infrastructure/DependencyInjection.cs`

```csharp
services.AddScoped<I{EntityName}Repository, {EntityName}Repository>();
```

---

## Reminders

- File-scoped namespaces everywhere.
- C# 13: primary constructors, collection expressions.
- All entity properties must have `HasColumnName("snake_case")`.
- Configure **both sides** of navigation relationships; set cascade behavior explicitly.
- If entity name is in Vietnamese, translate to English PascalCase and confirm with user.
