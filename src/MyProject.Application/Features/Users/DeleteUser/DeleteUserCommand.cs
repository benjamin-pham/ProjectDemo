using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Users.DeleteUser;

public sealed record DeleteUserCommand(Guid Id) : ICommand;
