using PersonalFinanceTracker.Domain.Enums;

namespace PersonalFinanceTracker.Application.Contracts.Transactions;

public sealed record CreateTransactionRequest(
    Guid CategoryId,
    decimal Amount,
    TransactionType Type,
    DateTime TransactionDateUtc,
    string? Note);
