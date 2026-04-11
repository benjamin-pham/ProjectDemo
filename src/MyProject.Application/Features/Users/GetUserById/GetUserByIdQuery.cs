using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Users.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserDetailResponse>;
