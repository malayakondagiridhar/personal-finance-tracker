using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Budgets;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BudgetsController(IBudgetService budgetService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var created = await budgetService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetStatus), new { userId = created.UserId, created.Year, created.Month }, created);
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus([FromQuery] Guid userId, [FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
    {
        var status = await budgetService.GetMonthlyStatusAsync(userId, year, month, cancellationToken);
        return Ok(status);
    }
}
