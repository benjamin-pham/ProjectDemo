using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;
using MyProject.Domain.Repositories;
using MyProject.Infrastructure.Data;

namespace MyProject.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        await Context.Users
            .FirstOrDefaultAsync(u => u.Username == username, ct);

    public override async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await Context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByHashedRefreshTokenAsync(string hashedToken, CancellationToken ct = default) =>
        await Context.Users
            .FirstOrDefaultAsync(u => u.HashedRefreshToken == hashedToken, ct);

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default) =>
        await Context.Users
            .AnyAsync(u => !u.IsDeleted && u.Username == username, ct);

    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default) =>
        await Context.Users
            .Include(u => u.Roles)
            .SingleOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);
}
