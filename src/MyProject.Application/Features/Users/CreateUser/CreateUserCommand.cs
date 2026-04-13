using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Users.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Username,
    string Password,
    string? Email,
    string? Phone,
    DateOnly? Birthday,
    List<Guid>? RoleIds = null) : ICommand<CreateUserResponse>;
