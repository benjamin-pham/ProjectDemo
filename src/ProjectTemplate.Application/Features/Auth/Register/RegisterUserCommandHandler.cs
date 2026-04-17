using ProjectTemplate.Application.Abstractions.Authentication;
using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Errors;
using ProjectTemplate.Domain.Repositories;

namespace ProjectTemplate.Application.Features.Auth.Register;

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
