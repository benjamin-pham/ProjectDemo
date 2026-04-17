using ProjectTemplate.Domain.Enumerations;

namespace ProjectTemplate.Application.Features.Roles.CreateRole;

public sealed record CreateRoleResponse(
    Guid Id,
    string Name,
    string Description,
    RoleType Type,
    List<string> Permissions);
