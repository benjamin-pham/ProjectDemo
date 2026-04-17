namespace ProjectTemplate.Application.Features.Roles.GetRoles;

public sealed record GetRolesResponse(
    Guid Id,
    string Name,
    string Description,
    string Type,
    string[] Permissions,
    DateTime CreatedAt);
