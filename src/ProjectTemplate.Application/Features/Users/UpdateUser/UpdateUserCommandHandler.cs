using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Errors;
using ProjectTemplate.Domain.Repositories;

namespace ProjectTemplate.Application.Features.Users.UpdateUser;

internal sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{

    public async Task<Result<UpdateUserResponse>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdWithRolesAsync(request.Id, cancellationToken);

        if (user is null)
            return Result.Failure<UpdateUserResponse>(UserErrors.NotFound);

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Birthday);

        if (request.RoleIds is not null)
        {
            var roles = await roleRepository.GetByIdsAsync(request.RoleIds, cancellationToken);

            if (roles.Count != request.RoleIds.Count)
                return Result.Failure<UpdateUserResponse>(RoleErrors.SomeNotFound);

            var rolesToRemove = user.Roles.Except(roles).ToList();
            foreach (var role in rolesToRemove)
                user.RemoveRole(role);

            foreach (var role in roles)
                user.AddRole(role);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedRoles = user.Roles
            .Select(r => new UpdatedRoleItem(r.Id, r.Name, r.Type.ToString()))
            .ToList();

        return new UpdateUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            user.Birthday,
            updatedRoles);
    }
}
