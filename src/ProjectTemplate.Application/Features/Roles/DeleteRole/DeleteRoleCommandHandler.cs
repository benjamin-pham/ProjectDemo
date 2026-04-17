using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Errors;
using ProjectTemplate.Domain.Repositories;

namespace ProjectTemplate.Application.Features.Roles.DeleteRole;

internal sealed class DeleteRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(
        DeleteRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdWithUsersAsync(request.Id, cancellationToken);

        if (role is null)
            return Result.Failure(RoleErrors.NotFound);

        if (role.Users.Count > 0)
            return Result.Failure(RoleErrors.HasActiveAssignments);

        roleRepository.Remove(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
