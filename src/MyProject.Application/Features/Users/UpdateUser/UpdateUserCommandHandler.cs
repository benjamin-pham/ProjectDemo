using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Users.UpdateUser;

internal sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    private static readonly Error UserNotFound =
        new("User.NotFound", "Người dùng không tồn tại.");

    public async Task<Result<UpdateUserResponse>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            return Result.Failure<UpdateUserResponse>(UserNotFound);

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Birthday);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            user.Birthday);
    }
}
