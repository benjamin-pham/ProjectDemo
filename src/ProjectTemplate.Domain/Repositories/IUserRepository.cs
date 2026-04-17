using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    new Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByHashedRefreshTokenAsync(string hashedToken, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);
}
