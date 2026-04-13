using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Roles.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string Description,
    string Type,
    List<string> Permissions) : ICommand<CreateRoleResponse>;
