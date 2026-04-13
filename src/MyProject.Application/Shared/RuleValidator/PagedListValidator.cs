using System;
using FluentValidation;
using MyProject.Domain.Abstractions;

namespace MyProject.Application.Shared.RuleValidator;

public abstract class PagedListValidator<TPagedListFilter> : AbstractValidator<TPagedListFilter> where TPagedListFilter : PagedListFilter
{
    protected PagedListValidator()
    {
        RuleFor(x => x.PageNumber)
            .NotNull().WithMessage("PageNumber không được để trống.")
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .NotNull().WithMessage("PageSize không được để trống.")
            .GreaterThanOrEqualTo(1).WithMessage("PageSize phải lớn hơn hoặc bằng 1.")
            .LessThanOrEqualTo(100).WithMessage("PageSize phải nhỏ hơn hoặc bằng 100.");
    }
}
