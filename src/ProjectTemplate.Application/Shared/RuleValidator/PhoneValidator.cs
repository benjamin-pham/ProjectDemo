using System;
using FluentValidation;

namespace ProjectTemplate.Application.Shared.RuleValidator;

public class PhoneValidator : AbstractValidator<string>
{
    public PhoneValidator()
    {
        RuleFor(x => x)
            .MinimumLength(7).WithMessage("Phone phải có ít nhất 7 ký tự.")
            .MaximumLength(15).WithMessage("Phone không được vượt quá 15 ký tự.")
            .Matches(@"^[0-9+]+$").WithMessage("Phone chỉ được chứa số và dấu +.")
            .Must(x => x.Count(c => c == '+') <= 1 && (x.Contains('+') ? x.StartsWith('+') : true)).WithMessage("Dấu + chỉ được nằm ở đầu số.");

    }
}