using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Application.Features.Users.GetUsers;

public sealed record GetUsersQuery : PagedListFilter, IQuery<PagedList<GetUsersResponse>>;
