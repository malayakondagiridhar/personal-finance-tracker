using PersonalFinanceTracker.Application.Contracts.Transactions;

namespace PersonalFinanceTracker.Application.Abstractions.Services;

public interface ITransactionService
{
    Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TransactionDto>> GetAsync(TransactionQuery query, CancellationToken cancellationToken = default);
    Task<TransactionDto> UpdateAsync(Guid transactionId, UpdateTransactionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid transactionId, CancellationToken cancellationToken = default);
}
