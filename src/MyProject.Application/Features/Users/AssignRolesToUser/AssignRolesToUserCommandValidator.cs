using FluentValidation;

namespace MyProject.Application.Features.Users.AssignRolesToUser;

public sealed class AssignRolesToUserCommandValidator : AbstractValidator<AssignRolesToUserCommand>
{
    public AssignRolesToUserCommandValidator()
    {
        RuleFor(x => x.RoleIds)
            .NotEmpty().WithMessage("Danh sách vai trò không được để trống.");

        RuleForEach(x => x.RoleIds)
            .NotEqual(Guid.Empty).WithMessage("Mỗi RoleId phải là một Guid hợp lệ.");
    }
}
