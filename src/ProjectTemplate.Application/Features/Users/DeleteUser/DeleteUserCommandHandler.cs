using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Errors;
using ProjectTemplate.Domain.Repositories;

namespace ProjectTemplate.Application.Features.Users.DeleteUser;

internal sealed class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteUserCommand>
{

    public async Task<Result> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        userRepository.Remove(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
