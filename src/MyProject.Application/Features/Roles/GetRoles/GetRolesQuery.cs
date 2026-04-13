using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Features.Users.GetUsers;

namespace MyProject.Application.Features.Roles.GetRoles;

public sealed record GetRolesQuery(int Page = 1, int PageSize = 20) : IQuery<PagedResponse<RoleListItemResponse>>;
