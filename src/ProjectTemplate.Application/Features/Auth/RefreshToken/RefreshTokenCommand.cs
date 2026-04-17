using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Application.Features.Auth.Login;
using ProjectTemplate.Application.Features.Auth.Shared;

namespace ProjectTemplate.Application.Features.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<TokenResponse>;
