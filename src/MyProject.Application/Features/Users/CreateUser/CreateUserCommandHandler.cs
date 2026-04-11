using MyProject.Application.Abstractions.Authentication;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Users.CreateUser;

internal sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    private static readonly Error UsernameAlreadyTaken =
        new("User.UsernameAlreadyTaken", "Username đã được sử dụng.");

    public async Task<Result<CreateUserResponse>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
            return Result.Failure<CreateUserResponse>(UsernameAlreadyTaken);

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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateUserResponse(user.Id, user.Username, user.FirstName, user.LastName);
    }
}
