using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Features.Auth.Shared;

namespace MyProject.Application.Features.Auth.Login;

public sealed record LoginUserCommand(
    string Username,
    string Password) : ICommand<TokenResponse>;


