namespace MyProject.Application.Features.Users.UpdateUser;

public sealed record UpdateUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateOnly? Birthday);
