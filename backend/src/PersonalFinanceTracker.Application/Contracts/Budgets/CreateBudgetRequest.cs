namespace PersonalFinanceTracker.Application.Contracts.Budgets;

public sealed record CreateBudgetRequest(
    Guid CategoryId,
    int Year,
    int Month,
    decimal LimitAmount);
