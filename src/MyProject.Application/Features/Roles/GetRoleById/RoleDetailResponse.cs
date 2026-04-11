namespace MyProject.Application.Features.Roles.GetRoleById;

public sealed record RoleDetailResponse(
    Guid Id,
    string Name,
    string Description,
    string Type,
    List<string> Permissions,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
