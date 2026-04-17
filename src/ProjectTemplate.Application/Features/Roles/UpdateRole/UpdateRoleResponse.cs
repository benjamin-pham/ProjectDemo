namespace ProjectTemplate.Application.Features.Roles.UpdateRole;

public sealed record UpdateRoleResponse(
    Guid Id,
    string Name,
    string Description,
    string Type,
    List<string> Permissions);
