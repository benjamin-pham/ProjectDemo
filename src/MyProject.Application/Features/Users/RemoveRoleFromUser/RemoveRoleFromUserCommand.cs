using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Users.RemoveRoleFromUser;

public sealed record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId) : ICommand;
