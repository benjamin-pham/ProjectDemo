using ProjectTemplate.Application.Abstractions.Messaging;

namespace ProjectTemplate.Application.Features.Users.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserDetailResponse>;
