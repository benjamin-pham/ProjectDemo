using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Errors;
using ProjectTemplate.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace ProjectTemplate.Application.Features.Auth.UpdateProfile;

internal sealed class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProfileCommandHandler> logger)
    : ICommandHandler<UpdateProfileCommand>
{

    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Birthday);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Profile updated for UserId={UserId} at {Timestamp}",
            userContext.UserId,
            DateTime.UtcNow);

        return Result.Success();
    }
}
