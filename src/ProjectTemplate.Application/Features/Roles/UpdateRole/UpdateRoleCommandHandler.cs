using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Enumerations;
using ProjectTemplate.Domain.Errors;
using ProjectTemplate.Domain.Repositories;

namespace ProjectTemplate.Application.Features.Roles.UpdateRole;

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

        role.Update(request.Name, request.Description, request.Type, request.Permissions);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateRoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.Type.ToString(),
            role.Permissions);
    }
}
