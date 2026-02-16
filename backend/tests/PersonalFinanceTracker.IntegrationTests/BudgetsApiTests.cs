using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PersonalFinanceTracker.Application.Contracts.Budgets;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Application.Contracts.Transactions;
using PersonalFinanceTracker.Domain.Enums;
using PersonalFinanceTracker.IntegrationTests.TestHost;

namespace PersonalFinanceTracker.IntegrationTests;

public sealed class BudgetsApiTests : IClassFixture<PersonalFinanceApiFactory>
{
    private readonly PersonalFinanceApiFactory _factory;

    public BudgetsApiTests(PersonalFinanceApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBudget_ThenGetStatus_ShouldReturnSpentAndRemainingAmounts()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var categoryResponse = await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(Guid.NewGuid(), "Food", "Groceries", false));
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        Assert.NotNull(category);

        var now = DateTime.UtcNow;
        var budgetRequest = new CreateBudgetRequest(Guid.NewGuid(), category!.Id, now.Year, now.Month, 2000m);
        var budgetResponse = await client.PostAsJsonAsync("/api/budgets", budgetRequest);

        Assert.Equal(HttpStatusCode.Created, budgetResponse.StatusCode);

        var expenseRequest = new CreateTransactionRequest(
            Guid.NewGuid(),
            category.Id,
            650m,
            TransactionType.Expense,
            new DateTime(now.Year, now.Month, 10, 10, 0, 0, DateTimeKind.Utc),
            "Weekly groceries");

        var transactionResponse = await client.PostAsJsonAsync("/api/transactions", expenseRequest);
        Assert.Equal(HttpStatusCode.Created, transactionResponse.StatusCode);

        var statusResponse = await client.GetAsync($"/api/budgets/status?year={now.Year}&month={now.Month}");
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);

        var statuses = await statusResponse.Content.ReadFromJsonAsync<List<BudgetStatusDto>>();
        Assert.NotNull(statuses);
        Assert.Single(statuses!);
        Assert.Equal(650m, statuses[0].SpentAmount);
        Assert.Equal(1350m, statuses[0].RemainingAmount);
    }

    [Fact]
    public async Task CreateBudget_DuplicateForSameMonth_ShouldReturnConflict()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var categoryResponse = await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(Guid.NewGuid(), "Rent", null, false));
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        Assert.NotNull(category);

        var now = DateTime.UtcNow;
        var request = new CreateBudgetRequest(Guid.NewGuid(), category!.Id, now.Year, now.Month, 15000m);

        var firstResponse = await client.PostAsJsonAsync("/api/budgets", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await client.PostAsJsonAsync("/api/budgets", request);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var payload = await secondResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(409, payload.GetProperty("status").GetInt32());
    }
}
