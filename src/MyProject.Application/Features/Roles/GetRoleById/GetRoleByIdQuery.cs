using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Roles.GetRoleById;

public sealed record GetRoleByIdQuery(Guid Id) : IQuery<RoleDetailResponse>;
