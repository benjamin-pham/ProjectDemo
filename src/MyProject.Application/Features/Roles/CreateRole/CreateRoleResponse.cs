namespace MyProject.Application.Features.Roles.CreateRole;

public sealed record CreateRoleResponse(
    Guid Id,
    string Name,
    string Description,
    string Type,
    List<string> Permissions);
