using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PersonalFinanceTracker.Application.Contracts.Categories;
using PersonalFinanceTracker.Application.Contracts.Common;
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

        var categories = await listResponse.Content.ReadFromJsonAsync<PagedResult<CategoryDto>>();
        Assert.NotNull(categories);
        Assert.Single(categories!.Items);
        Assert.Equal("Food", categories.Items[0].Name);
        Assert.Equal(userId, categories.Items[0].UserId);
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

    [Fact]
    public async Task GetAll_WithPaginationAndSort_ShouldReturnPagedSortedCategories()
    {
        var userId = Guid.NewGuid();
        var client = AuthenticatedClientFactory.Create(_factory, userId);

        await client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("Alpha", null, false));
        await client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("Beta", null, false));
        await client.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("Gamma", null, false));

        var response = await client.GetAsync("/api/v1/categories?page=1&pageSize=2&sortBy=name&sortDirection=desc");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<PagedResult<CategoryDto>>();
        Assert.NotNull(categories);
        Assert.Equal(2, categories!.Items.Count);
        Assert.Equal("Gamma", categories.Items[0].Name);
        Assert.Equal("Beta", categories.Items[1].Name);
    }

    [Fact]
    public async Task GetAll_ShouldNotLeakOtherUsersCategories()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var clientA = AuthenticatedClientFactory.Create(_factory, userA);
        var clientB = AuthenticatedClientFactory.Create(_factory, userB);

        await clientA.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("A-Only", null, false));
        await clientB.PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest("B-Only", null, false));

        var response = await clientA.GetAsync("/api/v1/categories?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<PagedResult<CategoryDto>>();
        Assert.NotNull(categories);
        Assert.All(categories!.Items, x => Assert.Equal(userA, x.UserId));
        Assert.DoesNotContain(categories.Items, x => x.Name == "B-Only");
    }
}



