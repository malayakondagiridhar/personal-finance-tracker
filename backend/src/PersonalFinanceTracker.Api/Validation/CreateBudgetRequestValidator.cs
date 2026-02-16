using FluentValidation;
using PersonalFinanceTracker.Application.Contracts.Budgets;

namespace PersonalFinanceTracker.Api.Validation;

public sealed class CreateBudgetRequestValidator : AbstractValidator<CreateBudgetRequest>
{
    public CreateBudgetRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.LimitAmount).GreaterThan(0);
    }
}
