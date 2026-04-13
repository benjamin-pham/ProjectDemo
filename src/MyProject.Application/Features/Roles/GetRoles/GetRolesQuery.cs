using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;

namespace MyProject.Application.Features.Roles.GetRoles;

public sealed record GetRolesQuery : PagedListFilter, IQuery<PagedList<GetRolesResponse>>;
