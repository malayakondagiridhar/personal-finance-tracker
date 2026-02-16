namespace PersonalFinanceTracker.Application.Contracts.Summaries;

public sealed record MonthlySummaryDto(
    Guid UserId,
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetSavings,
    IReadOnlyList<CategorySpendDto> CategoryBreakdown);
