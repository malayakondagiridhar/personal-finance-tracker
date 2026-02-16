using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Api.Auth;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Budgets;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Authorize(Policy = "FinanceApi")]
[Route("api/[controller]")]
public sealed class BudgetsController(IBudgetService budgetService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var sanitizedRequest = request with { UserId = userId };

        var created = await budgetService.CreateAsync(sanitizedRequest, cancellationToken);
        return CreatedAtAction(nameof(GetStatus), new { created.Year, created.Month }, created);
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus([FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var status = await budgetService.GetMonthlyStatusAsync(userId, year, month, cancellationToken);
        return Ok(status);
    }
}
