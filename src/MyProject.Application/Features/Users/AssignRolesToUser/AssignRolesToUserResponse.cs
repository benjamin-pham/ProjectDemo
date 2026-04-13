namespace MyProject.Application.Features.Users.AssignRolesToUser;

public sealed record AssignRolesToUserResponse(
    Guid UserId,
    IReadOnlyList<AssignedRoleItem> Roles);

public sealed record AssignedRoleItem(Guid Id, string Name, string Type);
