using PersonalFinanceTracker.Domain.Enums;

namespace PersonalFinanceTracker.Application.Contracts.Transactions;

public sealed record TransactionDto(
    Guid Id,
    Guid UserId,
    Guid CategoryId,
    decimal Amount,
    TransactionType Type,
    DateTime TransactionDateUtc,
    string? Note);
