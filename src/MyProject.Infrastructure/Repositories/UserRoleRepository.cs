using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;
using MyProject.Domain.Repositories;
using MyProject.Infrastructure.Data;

namespace MyProject.Infrastructure.Repositories;

public sealed class UserRoleRepository(AppDbContext context) : IUserRoleRepository
{
    public async Task<IReadOnlyList<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync(ct);

    public async Task AddAsync(UserRole userRole, CancellationToken ct = default) =>
        await context.UserRoles.AddAsync(userRole, ct);

    public void Remove(UserRole userRole) =>
        context.UserRoles.Remove(userRole);
}
