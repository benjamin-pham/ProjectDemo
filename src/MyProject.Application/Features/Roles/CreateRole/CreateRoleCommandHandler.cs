using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using MyProject.Domain.Enumerations;
using MyProject.Domain.Errors;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Roles.CreateRole;

internal sealed class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRoleCommand, CreateRoleResponse>
{

    public async Task<Result<CreateRoleResponse>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (await roleRepository.ExistsByNameAsync(request.Name, cancellationToken))
            return Result.Failure<CreateRoleResponse>(RoleErrors.NameAlreadyTaken);

        var roleType = Enum.Parse<RoleType>(request.Type);
        var role = Role.Create(request.Name, request.Description, roleType, request.Permissions);

        roleRepository.Add(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateRoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.Type.ToString(),
            role.Permissions);
    }
}
