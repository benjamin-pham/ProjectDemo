using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;

namespace MyProject.Domain.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
