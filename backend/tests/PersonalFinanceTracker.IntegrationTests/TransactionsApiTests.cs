using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Application.Contracts.Transactions;
using PersonalFinanceTracker.Domain.Enums;
using PersonalFinanceTracker.IntegrationTests.TestHost;

namespace PersonalFinanceTracker.IntegrationTests;

public sealed class TransactionsApiTests : IClassFixture<PersonalFinanceApiFactory>
{
    private readonly PersonalFinanceApiFactory _factory;

    public TransactionsApiTests(PersonalFinanceApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_Then_GetWithFilters_ShouldReturnTransaction()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var categoryResponse = await client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("Travel", "Cab and fuel", false));
        var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        Assert.NotNull(createdCategory);

        var transactionRequest = new CreateTransactionRequest(
            createdCategory!.Id,
            1200m,
            TransactionType.Expense,
            DateTime.UtcNow,
            "Airport cab");

        var createTransactionResponse = await client.PostAsJsonAsync("/api/v1/transactions", transactionRequest);
        Assert.Equal(HttpStatusCode.Created, createTransactionResponse.StatusCode);

        var fromDate = DateTime.UtcNow.AddDays(-1).ToString("O");
        var toDate = DateTime.UtcNow.AddDays(1).ToString("O");

        var getResponse = await client.GetAsync($"/api/v1/transactions?fromDateUtc={Uri.EscapeDataString(fromDate)}&toDateUtc={Uri.EscapeDataString(toDate)}&type={(int)TransactionType.Expense}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var transactions = await getResponse.Content.ReadFromJsonAsync<List<TransactionDto>>();
        Assert.NotNull(transactions);
        Assert.Single(transactions!);
        Assert.Equal(1200m, transactions[0].Amount);
        Assert.Equal(userId, transactions[0].UserId);
    }

    [Fact]
    public async Task Get_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var fromDate = DateTime.UtcNow.ToString("O");
        var toDate = DateTime.UtcNow.AddDays(-2).ToString("O");

        var response = await client.GetAsync($"/api/v1/transactions?fromDateUtc={Uri.EscapeDataString(fromDate)}&toDateUtc={Uri.EscapeDataString(toDate)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, payload.GetProperty("status").GetInt32());
    }
}

