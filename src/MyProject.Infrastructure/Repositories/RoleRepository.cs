using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;
using MyProject.Domain.Repositories;
using MyProject.Infrastructure.Data;

namespace MyProject.Infrastructure.Repositories;

public sealed class RoleRepository(AppDbContext context) : Repository<Role>(context), IRoleRepository
{
    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await Context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<IReadOnlyList<Role>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await Context.Roles
            .Where(r => r.Users.Any(u => u.Id == userId))
            .ToListAsync(ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        await Context.Roles
            .AnyAsync(r => !r.IsDeleted && r.Name == name, ct);

    public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken ct = default) =>
        await Context.Roles
            .AnyAsync(r => !r.IsDeleted && r.Name == name && r.Id != excludeId, ct);

    public async Task<IReadOnlyList<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
        await Context.Roles
            .Where(r => ids.Contains(r.Id) && !r.IsDeleted)
            .ToListAsync(ct);
}
