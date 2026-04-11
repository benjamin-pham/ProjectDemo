using MyProject.Domain.Entities;

namespace MyProject.Domain.Repositories;

public interface IUserRoleRepository
{
    Task<IReadOnlyList<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(UserRole userRole, CancellationToken ct = default);
    void Remove(UserRole userRole);
}
