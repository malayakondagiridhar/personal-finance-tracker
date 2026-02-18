using PersonalFinanceTracker.Domain.Enums;

namespace PersonalFinanceTracker.Application.Contracts.Transactions;

public sealed record TransactionQuery(
    Guid UserId,
    DateTime? FromDateUtc,
    DateTime? ToDateUtc,
    Guid? CategoryId,
    TransactionType? Type,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDirection);
