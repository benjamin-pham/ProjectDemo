using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Features.Auth.Login;
using MyProject.Application.Features.Auth.Shared;

namespace MyProject.Application.Features.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<TokenResponse>;
