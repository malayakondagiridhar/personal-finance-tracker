using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Api.Auth;
using PersonalFinanceTracker.Application.Abstractions.Services;
using PersonalFinanceTracker.Application.Contracts.Categories;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "FinanceApi")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var created = await categoryService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), null, created);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetRequiredUserId();
        var categories = await categoryService.GetAllAsync(userId, page, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(categories);
    }
}
