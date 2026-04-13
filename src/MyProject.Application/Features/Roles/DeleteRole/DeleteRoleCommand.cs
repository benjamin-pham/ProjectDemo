using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Roles.DeleteRole;

public sealed record DeleteRoleCommand(Guid Id) : ICommand;
