namespace PersonalFinanceTracker.Application.Contracts.Budgets;

public sealed record BudgetDto(
    Guid Id,
    Guid UserId,
    Guid CategoryId,
    int Year,
    int Month,
    decimal LimitAmount);
