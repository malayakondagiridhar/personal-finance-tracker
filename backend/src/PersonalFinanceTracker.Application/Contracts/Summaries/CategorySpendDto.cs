namespace PersonalFinanceTracker.Application.Contracts.Summaries;

public sealed record CategorySpendDto(
    Guid CategoryId,
    string CategoryName,
    decimal Amount);
