using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Users.AssignRolesToUser;

public sealed record AssignRolesToUserCommand(Guid UserId, List<Guid> RoleIds)
    : ICommand<AssignRolesToUserResponse>;
