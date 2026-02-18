using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Common;
using PersonalFinanceTracker.Application.Contracts.Transactions;
using PersonalFinanceTracker.Application.Exceptions;
using PersonalFinanceTracker.Domain.Entities;
using PersonalFinanceTracker.Infrastructure.Persistence;

namespace PersonalFinanceTracker.Infrastructure.Services;

public sealed class TransactionService(AppDbContext dbContext) : ITransactionService
{
    public async Task<TransactionDto> CreateAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCategoryBelongsToUserAsync(request.CategoryId, userId, cancellationToken);

        var transaction = new Transaction
        {
            UserId = userId,
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

    public async Task<PagedResult<TransactionDto>> GetAsync(TransactionQuery query, CancellationToken cancellationToken = default)
    {
        if (query.FromDateUtc.HasValue && query.ToDateUtc.HasValue && query.FromDateUtc > query.ToDateUtc)
        {
            throw new ValidationException("fromDateUtc cannot be greater than toDateUtc.");
        }

        if (query.Page < 1)
        {
            throw new ValidationException("page must be greater than or equal to 1.");
        }

        if (query.PageSize is < 1 or > 100)
        {
            throw new ValidationException("pageSize must be between 1 and 100.");
        }

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

        var normalizedSortBy = (query.SortBy ?? "transactionDateUtc").Trim().ToLowerInvariant();
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var sortedQuery = normalizedSortBy switch
        {
            "amount" => descending ? dataQuery.OrderByDescending(x => x.Amount) : dataQuery.OrderBy(x => x.Amount),
            "createdatutc" => descending ? dataQuery.OrderByDescending(x => x.CreatedAtUtc) : dataQuery.OrderBy(x => x.CreatedAtUtc),
            _ => descending ? dataQuery.OrderByDescending(x => x.TransactionDateUtc) : dataQuery.OrderBy(x => x.TransactionDateUtc)
        };

        var totalCount = await dataQuery.CountAsync(cancellationToken);
        var items = await sortedQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)query.PageSize);
        return new PagedResult<TransactionDto>(items, query.Page, query.PageSize, totalCount, totalPages);
    }

    public async Task<TransactionDto> UpdateAsync(Guid userId, Guid transactionId, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await dbContext.Transactions
            .FirstOrDefaultAsync(x => x.Id == transactionId && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException($"Transaction '{transactionId}' was not found.");

        await EnsureCategoryBelongsToUserAsync(request.CategoryId, userId, cancellationToken);

        transaction.CategoryId = request.CategoryId;
        transaction.Amount = request.Amount;
        transaction.Type = request.Type;
        transaction.TransactionDateUtc = request.TransactionDateUtc;
        transaction.Note = request.Note?.Trim();
        transaction.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(transaction);
    }

    public async Task DeleteAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await dbContext.Transactions
            .FirstOrDefaultAsync(x => x.Id == transactionId && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException($"Transaction '{transactionId}' was not found.");

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
            throw new NotFoundException("Category was not found for this user.");
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
