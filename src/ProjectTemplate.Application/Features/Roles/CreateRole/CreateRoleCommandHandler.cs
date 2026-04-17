using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Enumerations;
using ProjectTemplate.Domain.Errors;
using ProjectTemplate.Domain.Repositories;

namespace ProjectTemplate.Application.Features.Roles.CreateRole;

internal sealed class CreateRoleCommandHandler(
    IRoleRepository roleRepository)
    : ICommandHandler<CreateRoleCommand, CreateRoleResponse>
{
    public async Task<Result<CreateRoleResponse>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (await roleRepository.ExistsByNameAsync(request.Name, cancellationToken))
            return Result.Failure<CreateRoleResponse>(RoleErrors.NameAlreadyTaken);

        var role = Role.Create(request.Name, request.Description, request.Type, request.Permissions);

        roleRepository.Add(role);

        return new CreateRoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.Type,
            role.Permissions);
    }
}
