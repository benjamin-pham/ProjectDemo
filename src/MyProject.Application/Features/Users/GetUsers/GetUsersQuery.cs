using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;

namespace MyProject.Application.Features.Users.GetUsers;

public sealed record GetUsersQuery : PagedListFilter, IQuery<PagedList<GetUsersResponse>>;
