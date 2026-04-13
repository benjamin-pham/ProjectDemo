using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateOnly? Birthday,
    IReadOnlyList<Guid>? RoleIds) : ICommand<UpdateUserResponse>;
