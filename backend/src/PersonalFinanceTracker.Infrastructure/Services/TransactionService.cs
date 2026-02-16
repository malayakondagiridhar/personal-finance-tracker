using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Transactions;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Persistence;

namespace PersonalFinanceTracker.Infrastructure.Services;

public sealed class TransactionService(AppDbContext dbContext) : ITransactionService
{
    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCategoryBelongsToUserAsync(request.CategoryId, request.UserId, cancellationToken);

        var transaction = new Transaction
        {
            UserId = request.UserId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Type = request.Type,
            TransactionDateUtc = request.TransactionDateUtc,
            Note = request.Note?.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(transaction);
    }

    public async Task<IReadOnlyList<TransactionDto>> GetAsync(TransactionQuery query, CancellationToken cancellationToken = default)
    {
        var dataQuery = dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == query.UserId);

        if (query.FromDateUtc.HasValue)
        {
            dataQuery = dataQuery.Where(x => x.TransactionDateUtc >= query.FromDateUtc.Value);
        }

        if (query.ToDateUtc.HasValue)
        {
            dataQuery = dataQuery.Where(x => x.TransactionDateUtc <= query.ToDateUtc.Value);
        }

        if (query.CategoryId.HasValue)
        {
            dataQuery = dataQuery.Where(x => x.CategoryId == query.CategoryId.Value);
        }

        if (query.Type.HasValue)
        {
            dataQuery = dataQuery.Where(x => x.Type == query.Type.Value);
        }

        return await dataQuery
            .OrderByDescending(x => x.TransactionDateUtc)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TransactionDto> UpdateAsync(Guid transactionId, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await dbContext.Transactions
            .FirstOrDefaultAsync(x => x.Id == transactionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transaction '{transactionId}' was not found.");

        await EnsureCategoryBelongsToUserAsync(request.CategoryId, transaction.UserId, cancellationToken);

        transaction.CategoryId = request.CategoryId;
        transaction.Amount = request.Amount;
        transaction.Type = request.Type;
        transaction.TransactionDateUtc = request.TransactionDateUtc;
        transaction.Note = request.Note?.Trim();
        transaction.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(transaction);
    }

    public async Task DeleteAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await dbContext.Transactions
            .FirstOrDefaultAsync(x => x.Id == transactionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transaction '{transactionId}' was not found.");

        dbContext.Transactions.Remove(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCategoryBelongsToUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken)
    {
        var categoryExists = await dbContext.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Id == categoryId && x.UserId == userId, cancellationToken);

        if (!categoryExists)
        {
            throw new InvalidOperationException("Category was not found for this user.");
        }
    }

    private static TransactionDto ToDto(Transaction transaction)
    {
        return new TransactionDto(
            transaction.Id,
            transaction.UserId,
            transaction.CategoryId,
            transaction.Amount,
            transaction.Type,
            transaction.TransactionDateUtc,
            transaction.Note);
    }
}
