using ProjectTemplate.Application.Abstractions.Messaging;

namespace ProjectTemplate.Application.Features.Roles.DeleteRole;

public sealed record DeleteRoleCommand(Guid Id) : ICommand;
