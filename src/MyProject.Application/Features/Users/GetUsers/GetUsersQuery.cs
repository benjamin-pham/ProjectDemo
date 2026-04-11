using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Users.GetUsers;

public sealed record GetUsersQuery(int Page = 1, int PageSize = 20) : IQuery<PagedResponse<UserListItemResponse>>;
