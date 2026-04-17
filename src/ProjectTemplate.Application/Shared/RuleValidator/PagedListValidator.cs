using System;
using FluentValidation;
using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Application.Shared.RuleValidator;

public abstract class PagedListValidator<TPagedListFilter> : AbstractValidator<TPagedListFilter> where TPagedListFilter : PagedListFilter
{
    protected PagedListValidator()
    {
        When(x => x.PageNumber is not null, () =>
            RuleFor(x => x.PageNumber!.Value)
                .GreaterThanOrEqualTo(1)
                .WithMessage("PageNumber phải lớn hơn hoặc bằng 1."));

        When(x => x.PageSize is not null, () =>
            RuleFor(x => x.PageSize!.Value)
                .GreaterThanOrEqualTo(1)
                .WithMessage("PageSize phải lớn hơn hoặc bằng 1.")
                .LessThanOrEqualTo(100)
                .WithMessage("PageSize phải nhỏ hơn hoặc bằng 100."));
    }
}
