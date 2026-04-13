using MyProject.Application.Abstractions.Authentication;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using MyProject.Domain.Errors;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Auth.Register;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{

    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existing is not null)
            return Result.Failure<RegisterUserResponse>(UserErrors.UsernameAlreadyTaken);

        var passwordHash = passwordHasher.Hash(request.Password);

        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Username,
            passwordHash,
            request.Email,
            request.Phone,
            request.Birthday);

        userRepository.Add(user);

        return new RegisterUserResponse(user.Id, user.Username, user.FirstName, user.LastName);
    }
}
