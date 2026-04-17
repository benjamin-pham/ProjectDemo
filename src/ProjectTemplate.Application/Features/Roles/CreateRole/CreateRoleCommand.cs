using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Enumerations;

namespace ProjectTemplate.Application.Features.Roles.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string Description,
    RoleType Type,
    List<string> Permissions) : ICommand<CreateRoleResponse>;
