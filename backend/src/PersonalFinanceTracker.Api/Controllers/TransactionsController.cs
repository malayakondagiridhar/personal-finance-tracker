using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Api.Auth;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Transactions;
using PersonalFinanceTracker.Domain.Enums;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "FinanceApi")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class TransactionsController(ITransactionService transactionService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetRequiredUserId();
        var created = await transactionService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), null, created);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime? fromDateUtc,
        [FromQuery] DateTime? toDateUtc,
        [FromQuery] Guid? categoryId,
        [FromQuery] TransactionType? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "transactionDateUtc",
        [FromQuery] string? sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetRequiredUserId();
        var query = new TransactionQuery(userId, fromDateUtc, toDateUtc, categoryId, type, page, pageSize, sortBy, sortDirection);
        var data = await transactionService.GetAsync(query, cancellationToken);
        return Ok(data);
    }

    [HttpPut("{transactionId:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid transactionId, [FromBody] UpdateTransactionRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetRequiredUserId();
        var updated = await transactionService.UpdateAsync(userId, transactionId, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{transactionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid transactionId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetRequiredUserId();
        await transactionService.DeleteAsync(userId, transactionId, cancellationToken);
        return NoContent();
    }
}
