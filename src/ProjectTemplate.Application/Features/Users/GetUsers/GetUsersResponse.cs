namespace ProjectTemplate.Application.Features.Users.GetUsers;

public sealed record GetUsersResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Username,
    string? Email,
    string? Phone,
    DateOnly? Birthday,
    DateTime CreatedAt);
