namespace MyProject.Application.Features.Users.CreateUser;

public sealed record CreateUserResponse(
    Guid Id,
    string Username,
    string FirstName,
    string LastName,
    IReadOnlyList<AssignedRoleItem> Roles);

public sealed record AssignedRoleItem(Guid Id, string Name);
