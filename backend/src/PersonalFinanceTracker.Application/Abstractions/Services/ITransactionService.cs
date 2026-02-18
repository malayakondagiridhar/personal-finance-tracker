using PersonalFinanceTracker.Application.Contracts.Transactions;

namespace PersonalFinanceTracker.Application.Abstractions.Services;

public interface ITransactionService
{
    Task<TransactionDto> CreateAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TransactionDto>> GetAsync(TransactionQuery query, CancellationToken cancellationToken = default);
    Task<TransactionDto> UpdateAsync(Guid userId, Guid transactionId, UpdateTransactionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);
}
