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
            .Where(r => Context.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == r.Id))
            .ToListAsync(ct);
}
