using FluentValidation;

namespace ProjectTemplate.Application.Features.Roles.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên vai trò không được để trống.")
            .MaximumLength(100).WithMessage("Tên vai trò không được vượt quá 100 ký tự.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Mô tả vai trò không được để trống.")
            .MaximumLength(500).WithMessage("Mô tả vai trò không được vượt quá 500 ký tự.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Loại vai trò không được để trống.");

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("Danh sách quyền không được null.");
    }
}
