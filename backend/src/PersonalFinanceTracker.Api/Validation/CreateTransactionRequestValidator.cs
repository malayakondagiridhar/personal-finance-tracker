using FluentValidation;
using PersonalFinanceTracker.Application.Contracts.Transactions;

namespace PersonalFinanceTracker.Api.Validation;

public sealed class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.TransactionDateUtc).LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
