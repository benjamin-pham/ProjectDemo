using ProjectTemplate.Application.Abstractions.Messaging;

namespace ProjectTemplate.Application.Features.Users.DeleteUser;

public sealed record DeleteUserCommand(Guid Id) : ICommand;
