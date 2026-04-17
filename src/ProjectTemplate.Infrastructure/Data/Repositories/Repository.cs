using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Infrastructure.Data;

namespace ProjectTemplate.Infrastructure.Data.Repositories;

public abstract class Repository<T, TKey>(AppDbContext context) : IRepository<T, TKey>
    where T : BaseEntity<TKey>
    where TKey : notnull
{
    protected readonly AppDbContext Context = context;

    public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken ct = default) =>
        await Context.Set<T>().FindAsync([id!], ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await Context.Set<T>().ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Context.Set<T>().AddAsync(entity, ct);

    public virtual void Update(T entity) =>
        Context.Set<T>().Update(entity);

    public virtual void Remove(T entity) =>
        Context.Set<T>().Remove(entity);

    public virtual void Add(T entity) =>
        Context.Set<T>().Add(entity);
}

// Shorthand for the common case — Guid primary key
public abstract class Repository<T>(AppDbContext context) : Repository<T, Guid>(context)
    where T : BaseEntity
{
}
