using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Application.Features.Roles.GetRoles;

public sealed record GetRolesQuery : PagedListFilter, IQuery<PagedList<GetRolesResponse>>;
