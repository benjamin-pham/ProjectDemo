using MyProject.Domain.Abstractions;
using MyProject.Domain.Enumerations;

namespace MyProject.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoleType Type { get; set; }
    public List<string> Permissions { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
