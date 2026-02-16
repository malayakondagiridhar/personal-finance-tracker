using FluentValidation;
using PersonalFinanceTracker.Application.Contracts.Categories;

namespace PersonalFinanceTracker.Api.Validation;

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}
