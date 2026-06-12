using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Master.ProductCategories.Queries.GetProductCategories;
using AeroMes.Application.Master.ProductCategories.Queries.GetProductCategoryTree;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class ProductCategoryTests(AeroMesWebFactory factory)
{
    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_WithSchedulingMetadata_PersistsAllFields()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("GRP");

        var response = await client.PostAsJsonAsync("/api/v1/product-categories", new
        {
            Code = code,
            Name = "Stamped Parts",
            Description = "Sheet-metal stamped components",
            StandardProductionTime = 2.5m,
            Color = "#FF8800",
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var list = await client.GetFromJsonAsync<List<ProductCategoryDto>>("/api/v1/product-categories");
        var created = list!.Single(c => c.CategoryCode == code);
        Assert.Equal("Stamped Parts", created.CategoryName);
        Assert.Equal("Sheet-metal stamped components", created.Description);
        Assert.Equal(2.5m, created.StandardProductionTime);
        Assert.Equal("#FF8800", created.Color);
    }

    [Fact]
    public async Task Tree_ReturnsNestedHierarchy()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var rootCode = UniqueCode("RT");
        var childCode = UniqueCode("CH");

        var rootId = await CreateCategoryAsync(client, rootCode, "Root Group");
        await CreateCategoryAsync(client, childCode, "Child Group", parentId: rootId);

        var tree = await client.GetFromJsonAsync<List<ProductCategoryTreeDto>>("/api/v1/product-categories/tree");
        var root = tree!.Single(n => n.CategoryCode == rootCode);
        var child = Assert.Single(root.Children);
        Assert.Equal(childCode, child.CategoryCode);
    }

    [Fact]
    public async Task Update_SetParentToOwnDescendant_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var parentId = await CreateCategoryAsync(client, UniqueCode("CYA"), "Cycle A");
        var childId = await CreateCategoryAsync(client, UniqueCode("CYB"), "Cycle B", parentId: parentId);

        var response = await client.PutAsJsonAsync($"/api/v1/product-categories/{parentId}", new
        {
            ParentId = childId,
            Name = "Cycle A",
            Description = (string?)null,
            StandardProductionTime = (decimal?)null,
            Color = (string?)null,
            IsActive = true,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Delete_CategoryWithChildren_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var parentId = await CreateCategoryAsync(client, UniqueCode("DPA"), "Parent");
        await CreateCategoryAsync(client, UniqueCode("DCH"), "Child", parentId: parentId);

        var response = await client.DeleteAsync($"/api/v1/product-categories/{parentId}");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Delete_CategoryWithProducts_Returns422_DeactivationStillAllowed()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var categoryId = await CreateCategoryAsync(client, UniqueCode("DPR"), "Has Products");
        await CreateProductAsync(client, categoryId);

        var delete = await client.DeleteAsync($"/api/v1/product-categories/{categoryId}");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, delete.StatusCode);

        var deactivate = await client.PutAsJsonAsync($"/api/v1/product-categories/{categoryId}", new
        {
            ParentId = (int?)null,
            Name = "Has Products",
            Description = (string?)null,
            StandardProductionTime = (decimal?)null,
            Color = (string?)null,
            IsActive = false,
        });
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);
    }

    [Fact]
    public async Task Delete_EmptyCategory_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateCategoryAsync(client, UniqueCode("DEL"), "Disposable");

        var response = await client.DeleteAsync($"/api/v1/product-categories/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithInactiveCategory_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var categoryId = await CreateCategoryAsync(client, UniqueCode("INA"), "Inactive Group");

        var deactivate = await client.PutAsJsonAsync($"/api/v1/product-categories/{categoryId}", new
        {
            ParentId = (int?)null,
            Name = "Inactive Group",
            Description = (string?)null,
            StandardProductionTime = (decimal?)null,
            Color = (string?)null,
            IsActive = false,
        });
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);

        var response = await TryCreateProductAsync(client, categoryId);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private static async Task<int> CreateCategoryAsync(HttpClient client, string code, string name, int? parentId = null)
    {
        var response = await client.PostAsJsonAsync("/api/v1/product-categories",
            new { Code = code, Name = name, ParentId = parentId });
        Assert.True(response.IsSuccessStatusCode, $"Category create failed: {await response.Content.ReadAsStringAsync()}");
        var created = await response.Content.ReadFromJsonAsync<CreatedResult>();
        return created!.CategoryId;
    }

    private static async Task<HttpResponseMessage> TryCreateProductAsync(HttpClient client, int categoryId)
    {
        var uomCode = UniqueCode("EA")[..8];
        var uomResp = await client.PostAsJsonAsync("/api/v1/uom",
            new { Code = uomCode, Name = $"Each {uomCode}", Group = "Quantity" });
        Assert.True(uomResp.IsSuccessStatusCode, $"UoM create failed: {await uomResp.Content.ReadAsStringAsync()}");

        var productCode = UniqueCode("PRD");
        return await client.PostAsJsonAsync("/api/v1/products",
            new { Code = productCode, Name = $"Product {productCode}", BaseUoMCode = uomCode, CategoryId = categoryId });
    }

    private static async Task<string> CreateProductAsync(HttpClient client, int categoryId)
    {
        var response = await TryCreateProductAsync(client, categoryId);
        Assert.True(response.IsSuccessStatusCode, $"Product create failed: {await response.Content.ReadAsStringAsync()}");
        return string.Empty;
    }

    private record CreatedResult(int CategoryId);
}
