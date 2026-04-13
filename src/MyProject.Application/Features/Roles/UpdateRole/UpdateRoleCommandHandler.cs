using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Enumerations;
using MyProject.Domain.Errors;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Roles.UpdateRole;

internal sealed class UpdateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateRoleCommand, UpdateRoleResponse>
{

    public async Task<Result<UpdateRoleResponse>> Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (role is null)
            return Result.Failure<UpdateRoleResponse>(RoleErrors.NotFound);

        if (await roleRepository.ExistsByNameExcludingIdAsync(request.Name, request.Id, cancellationToken))
            return Result.Failure<UpdateRoleResponse>(RoleErrors.NameAlreadyTaken);

        var roleType = Enum.Parse<RoleType>(request.Type);
        role.Update(request.Name, request.Description, roleType, request.Permissions);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateRoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.Type.ToString(),
            role.Permissions);
    }
}
