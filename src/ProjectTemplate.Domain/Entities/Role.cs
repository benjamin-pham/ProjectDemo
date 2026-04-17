using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Enumerations;

namespace ProjectTemplate.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoleType Type { get; set; }
    public List<string> Permissions { get; set; } = [];
    public ICollection<User> Users { get; set; } = [];

    public Role() { }

    public static Role Create(string name, string description, RoleType type, List<string> permissions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Role
        {
            Name = name,
            Description = description,
            Type = type,
            Permissions = permissions
        };
    }

    public void Update(string name, string description, RoleType type, List<string> permissions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Description = description;
        Type = type;
        Permissions = permissions;
    }
}
