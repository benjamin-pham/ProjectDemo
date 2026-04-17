namespace ProjectTemplate.Application.Features.Users.GetUserById;

public sealed record UserDetailResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Username,
    string? Email,
    string? Phone,
    DateOnly? Birthday,
    DateTime CreatedAt,
    IReadOnlyList<UserRoleItem> Roles);

public sealed record UserRoleItem(Guid Id, string Name, string Type);
