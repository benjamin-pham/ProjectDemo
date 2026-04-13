using MyProject.Application.Abstractions.Authentication;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using MyProject.Domain.Errors;
using MyProject.Domain.Repositories;

namespace MyProject.Application.Features.Users.CreateUser;

internal sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUserCommand, CreateUserResponse>
{

    public async Task<Result<CreateUserResponse>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
            return Result.Failure<CreateUserResponse>(UserErrors.UsernameAlreadyTaken);

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Username,
            passwordHash,
            request.Email,
            request.Phone,
            request.Birthday);

        if (request.RoleIds is { Count: > 0 })
        {
            var roles = await roleRepository.GetByIdsAsync(request.RoleIds, cancellationToken);

            if (roles.Count != request.RoleIds.Count)
                return Result.Failure<CreateUserResponse>(RoleErrors.SomeNotFound);

            foreach (var role in roles)
                user.AddRole(role);
        }

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var assignedRoles = user.Roles
            .Select(r => new AssignedRoleItem(r.Id, r.Name))
            .ToList();

        return new CreateUserResponse(user.Id, user.Username, user.FirstName, user.LastName, assignedRoles);
    }
}
