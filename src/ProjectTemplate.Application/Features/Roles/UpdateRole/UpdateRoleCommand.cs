using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Enumerations;

namespace ProjectTemplate.Application.Features.Roles.UpdateRole;

public sealed record UpdateRoleCommand(
    Guid Id,
    string Name,
    string Description,
    RoleType Type,
    List<string> Permissions) : ICommand<UpdateRoleResponse>;
