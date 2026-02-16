using System.Net;
using System.Net.Http.Json;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Application.Contracts.Summaries;
using PersonalFinanceTracker.Application.Contracts.Transactions;
using PersonalFinanceTracker.Domain.Enums;
using PersonalFinanceTracker.IntegrationTests.TestHost;

namespace PersonalFinanceTracker.IntegrationTests;

public sealed class SummaryApiTests : IClassFixture<PersonalFinanceApiFactory>
{
    private readonly HttpClient _client;
    private readonly PersonalFinanceApiFactory _factory;

    public SummaryApiTests(PersonalFinanceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetMonthlySummary_ShouldReturnIncomeExpenseNetAndBreakdown()
    {
        var userId = Guid.NewGuid();
        await TestDataSeeder.SeedUserAsync(_factory, userId);

        var foodCategoryResponse = await _client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(userId, "Food", null, false));
        var foodCategory = await foodCategoryResponse.Content.ReadFromJsonAsync<CategoryDto>();

        var salaryCategoryResponse = await _client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(userId, "Salary", null, false));
        var salaryCategory = await salaryCategoryResponse.Content.ReadFromJsonAsync<CategoryDto>();

        Assert.Equal(HttpStatusCode.Created, foodCategoryResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, salaryCategoryResponse.StatusCode);
        Assert.NotNull(foodCategory);
        Assert.NotNull(salaryCategory);

        var now = DateTime.UtcNow;

        var incomeResponse = await _client.PostAsJsonAsync("/api/transactions", new CreateTransactionRequest(
            userId,
            salaryCategory!.Id,
            5000m,
            TransactionType.Income,
            new DateTime(now.Year, now.Month, 2, 9, 0, 0, DateTimeKind.Utc),
            "Monthly salary"));

        var expenseResponse = await _client.PostAsJsonAsync("/api/transactions", new CreateTransactionRequest(
            userId,
            foodCategory!.Id,
            1200m,
            TransactionType.Expense,
            new DateTime(now.Year, now.Month, 5, 20, 0, 0, DateTimeKind.Utc),
            "Groceries"));

        Assert.Equal(HttpStatusCode.Created, incomeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, expenseResponse.StatusCode);

        var summaryResponse = await _client.GetAsync($"/api/summary/monthly?userId={userId}&year={now.Year}&month={now.Month}");
        var summaryBody = await summaryResponse.Content.ReadAsStringAsync();
        Assert.True(summaryResponse.StatusCode == HttpStatusCode.OK, $"Expected 200 but got {(int)summaryResponse.StatusCode}: {summaryBody}");

        var summary = System.Text.Json.JsonSerializer.Deserialize<MonthlySummaryDto>(summaryBody, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(summary);
        Assert.Equal(5000m, summary!.TotalIncome);
        Assert.Equal(1200m, summary.TotalExpense);
        Assert.Equal(3800m, summary.NetSavings);
        Assert.Single(summary.CategoryBreakdown);
        Assert.Equal("Food", summary.CategoryBreakdown[0].CategoryName);
        Assert.Equal(1200m, summary.CategoryBreakdown[0].Amount);
    }
}
