using ProjectTemplate.Application.Abstractions.Messaging;

namespace ProjectTemplate.Application.Features.Auth.GetProfile;

public sealed record GetProfileQuery : IQuery<UserProfileResponse>;


