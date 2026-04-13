using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Roles.UpdateRole;

public sealed record UpdateRoleCommand(
    Guid Id,
    string Name,
    string Description,
    string Type,
    List<string> Permissions) : ICommand<UpdateRoleResponse>;
