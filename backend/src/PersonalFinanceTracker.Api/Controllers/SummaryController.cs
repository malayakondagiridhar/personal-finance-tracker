using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Api.Auth;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Summaries;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "FinanceApi")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class SummaryController(ISummaryService summaryService) : ControllerBase
{
    [HttpGet("monthly")]
    [ProducesResponseType(typeof(MonthlySummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthly([FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var summary = await summaryService.GetMonthlySummaryAsync(userId, year, month, cancellationToken);
        return Ok(summary);
    }
}
