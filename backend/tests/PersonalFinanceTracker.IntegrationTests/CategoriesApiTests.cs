using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.IntegrationTests.TestHost;

namespace PersonalFinanceTracker.IntegrationTests;

public sealed class CategoriesApiTests : IClassFixture<PersonalFinanceApiFactory>
{
    private readonly PersonalFinanceApiFactory _factory;

    public CategoriesApiTests(PersonalFinanceApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_Then_GetAll_ShouldReturnCreatedCategory()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var createRequest = new CreateCategoryRequest("Food", "Groceries", false);
        var createResponse = await client.PostAsJsonAsync("/api/v1/categories", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await client.GetAsync("/api/v1/categories");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var categories = await listResponse.Content.ReadFromJsonAsync<List<CategoryDto>>();
        Assert.NotNull(categories);
        Assert.Single(categories!);
        Assert.Equal("Food", categories[0].Name);
        Assert.Equal(userId, categories[0].UserId);
    }

    [Fact]
    public async Task Create_DuplicateNameForSameUser_ShouldReturnConflict()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        var request = new CreateCategoryRequest("Utilities", null, false);

        var firstResponse = await client.PostAsJsonAsync("/api/v1/categories", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("utilities", null, false));
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var payload = await secondResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(409, payload.GetProperty("status").GetInt32());
    }
}



