using ProjectTemplate.Application.Abstractions.Messaging;

namespace ProjectTemplate.Application.Features.Roles.GetRoleById;

public sealed record GetRoleByIdQuery(Guid Id) : IQuery<RoleDetailResponse>;
