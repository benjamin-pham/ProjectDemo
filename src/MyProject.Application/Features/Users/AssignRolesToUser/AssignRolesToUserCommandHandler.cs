using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Errors;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Users.AssignRolesToUser;

internal sealed class AssignRolesToUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignRolesToUserCommand, AssignRolesToUserResponse>
{

    public async Task<Result<AssignRolesToUserResponse>> Handle(
        AssignRolesToUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<AssignRolesToUserResponse>(UserErrors.NotFound);

        var roles = await roleRepository.GetByIdsAsync(request.RoleIds, cancellationToken);

        if (roles.Count != request.RoleIds.Count)
            return Result.Failure<AssignRolesToUserResponse>(RoleErrors.SomeNotFound);

        foreach (var role in roles)
            user.AddRole(role);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var assignedRoles = user.Roles
            .Select(r => new AssignedRoleItem(r.Id, r.Name, r.Type.ToString()))
            .ToList();

        return new AssignRolesToUserResponse(user.Id, assignedRoles);
    }
}
