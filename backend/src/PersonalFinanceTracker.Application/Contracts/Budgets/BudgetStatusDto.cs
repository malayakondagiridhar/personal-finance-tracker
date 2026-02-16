namespace PersonalFinanceTracker.Application.Contracts.Budgets;

public sealed record BudgetStatusDto(
    Guid BudgetId,
    Guid CategoryId,
    string CategoryName,
    decimal LimitAmount,
    decimal SpentAmount,
    decimal RemainingAmount);
