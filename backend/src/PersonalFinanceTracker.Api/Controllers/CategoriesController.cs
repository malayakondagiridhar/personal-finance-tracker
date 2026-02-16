using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Api.Auth;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Categories;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Authorize(Policy = "FinanceApi")]
[Route("api/[controller]")]
public sealed class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var sanitizedRequest = request with { UserId = userId };

        var created = await categoryService.CreateAsync(sanitizedRequest, cancellationToken);
        return CreatedAtAction(nameof(GetAll), null, created);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var categories = await categoryService.GetAllAsync(userId, cancellationToken);
        return Ok(categories);
    }
}
