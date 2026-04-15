using FluentValidation;
using MyProject.Application.Shared.RuleValidator;

namespace MyProject.Application.Features.Users.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName không được để trống.")
            .MaximumLength(100).WithMessage("FirstName không được vượt quá 100 ký tự.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName không được để trống.")
            .MaximumLength(100).WithMessage("LastName không được vượt quá 100 ký tự.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username không được để trống.")
            .SetValidator(new UsernameValidator());

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password không được để trống.")
            .SetValidator(new PasswordValidator());

        When(x => x.Email is not null, () =>
            RuleFor(x => x.Email!)
                .EmailAddress().WithMessage("Email không đúng định dạng."));

        When(x => x.Phone is not null, () =>
            RuleFor(x => x.Phone!)
                .SetValidator(new PhoneValidator()));

        When(x => x.Birthday.HasValue, () =>
            RuleFor(x => x.Birthday!.Value)
                .Must(b => b <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Birthday không được là ngày tương lai."));

        When(x => x.RoleIds is { Count: > 0 }, () =>
            RuleForEach(x => x.RoleIds)
                .NotEmpty().WithMessage("RoleId không được là Guid rỗng."));
    }
}
