using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Application.Features.Auth.Shared;

namespace ProjectTemplate.Application.Features.Auth.Login;

public sealed record LoginUserCommand(
    string Username,
    string Password) : ICommand<TokenResponse>;


