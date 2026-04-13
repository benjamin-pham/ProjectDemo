using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Errors;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Users.RemoveRoleFromUser;

internal sealed class RemoveRoleFromUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveRoleFromUserCommand>
{

    public async Task<Result> Handle(
        RemoveRoleFromUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        var role = user.Roles.FirstOrDefault(r => r.Id == request.RoleId);

        if (role is null)
            return Result.Failure(RoleErrors.NotAssigned);

        user.RemoveRole(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
