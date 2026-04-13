namespace MyProject.Application.Features.Roles.GetRoles;

public sealed record RoleListItemResponse(
    Guid Id,
    string Name,
    string Description,
    string Type,
    List<string> Permissions,
    DateTime CreatedAt);
