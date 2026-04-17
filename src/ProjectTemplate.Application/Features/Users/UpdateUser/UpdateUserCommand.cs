using ProjectTemplate.Application.Abstractions.Messaging;

namespace ProjectTemplate.Application.Features.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateOnly? Birthday,
    IReadOnlyList<Guid>? RoleIds) : ICommand<UpdateUserResponse>;
