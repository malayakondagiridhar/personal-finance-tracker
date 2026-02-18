using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Api.Auth;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Budgets;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "FinanceApi")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class BudgetsController(IBudgetService budgetService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetRequiredUserId();
        var created = await budgetService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetStatus), new { created.Year, created.Month }, created);
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus([FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetRequiredUserId();
        var status = await budgetService.GetMonthlyStatusAsync(userId, year, month, cancellationToken);
        return Ok(status);
    }
}
