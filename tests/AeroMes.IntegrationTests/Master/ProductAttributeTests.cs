using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeById;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributes;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeAssignments;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class ProductAttributeTests(AeroMesWebFactory factory)
{
    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/product-attributes");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithValues_Returns201AndDetailContainsValues()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("ATR");

        var response = await client.PostAsJsonAsync("/api/v1/product-attributes", new
        {
            Code = code,
            Name = "Color",
            Values = new[]
            {
                new { Value = "Red", GroupName = "Primary Colors", SortOrder = 1 },
                new { Value = "Blue", GroupName = "Primary Colors", SortOrder = 2 },
                new { Value = "Teal", GroupName = "Extended Colors", SortOrder = 3 },
            },
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatedResult>();
        Assert.NotNull(created);

        var detail = await client.GetFromJsonAsync<ProductAttributeDetailDto>(
            $"/api/v1/product-attributes/{created.AttributeId}");
        Assert.NotNull(detail);
        Assert.Equal(code, detail.AttributeCode);
        Assert.Equal(3, detail.Values.Count);
        Assert.Equal(["Red", "Blue", "Teal"], detail.Values.Select(v => v.Value).ToArray());
    }

    [Fact]
    public async Task Create_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DUP");

        await client.PostAsJsonAsync("/api/v1/product-attributes", new { Code = code, Name = "Size" });
        var response = await client.PostAsJsonAsync("/api/v1/product-attributes", new { Code = code, Name = "Size again" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateValuesInPayload_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/product-attributes", new
        {
            Code = UniqueCode("DVL"),
            Name = "Size",
            Values = new[]
            {
                new { Value = "XL", GroupName = (string?)null, SortOrder = 1 },
                new { Value = "xl", GroupName = (string?)null, SortOrder = 2 },
            },
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task AddValue_Duplicate_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateAttributeAsync(client, "Material", [("Cotton", null, 1)]);

        var ok = await client.PostAsJsonAsync($"/api/v1/product-attributes/{id}/values",
            new { Value = "Polyester", SortOrder = 2 });
        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);

        var dup = await client.PostAsJsonAsync($"/api/v1/product-attributes/{id}/values",
            new { Value = "cotton", SortOrder = 3 });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, dup.StatusCode);
    }

    [Fact]
    public async Task UpdateAndRemoveValue_Succeed()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateAttributeAsync(client, "Finish", [("Matte", null, 1), ("Gloss", null, 2)]);

        var detail = await client.GetFromJsonAsync<ProductAttributeDetailDto>($"/api/v1/product-attributes/{id}");
        var matte = detail!.Values.Single(v => v.Value == "Matte");

        var update = await client.PutAsJsonAsync($"/api/v1/product-attributes/{id}/values/{matte.ValueId}",
            new { Value = "Satin", GroupName = "Sheen", SortOrder = 1 });
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var remove = await client.DeleteAsync(
            $"/api/v1/product-attributes/{id}/values/{detail.Values.Single(v => v.Value == "Gloss").ValueId}");
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);

        var after = await client.GetFromJsonAsync<ProductAttributeDetailDto>($"/api/v1/product-attributes/{id}");
        var value = Assert.Single(after!.Values);
        Assert.Equal("Satin", value.Value);
        Assert.Equal("Sheen", value.GroupName);
    }

    [Fact]
    public async Task Update_DeactivateAttribute_HiddenFromActiveList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DEA");
        var id = await CreateAttributeAsync(client, "Weight Class", [], code);

        var update = await client.PutAsJsonAsync($"/api/v1/product-attributes/{id}",
            new { Name = "Weight Class", IsActive = false });
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var active = await client.GetFromJsonAsync<List<ProductAttributeDto>>("/api/v1/product-attributes");
        Assert.DoesNotContain(active!, a => a.AttributeId == id);

        var all = await client.GetFromJsonAsync<List<ProductAttributeDto>>("/api/v1/product-attributes?activeOnly=false");
        Assert.Contains(all!, a => a.AttributeId == id);
    }

    [Fact]
    public async Task ValueGroups_ReturnsDistinctGroupNames()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var group = $"Group {UniqueCode("VG")}";
        await CreateAttributeAsync(client, "Grouped Attr", [("A", group, 1), ("B", group, 2)]);

        var groups = await client.GetFromJsonAsync<List<string>>("/api/v1/product-attributes/value-groups");
        Assert.NotNull(groups);
        Assert.Single(groups, g => g == group);
    }

    [Fact]
    public async Task Assign_ToProduct_WithSelectedValue_Succeeds()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var productCode = await CreateProductAsync(client);
        var id = await CreateAttributeAsync(client, "Color", [("Red", null, 1), ("Blue", null, 2)]);

        var detail = await client.GetFromJsonAsync<ProductAttributeDetailDto>($"/api/v1/product-attributes/{id}");
        var red = detail!.Values.Single(v => v.Value == "Red");

        var assign = await client.PutAsJsonAsync($"/api/v1/products/{productCode}/attributes/{id}",
            new { SelectedValueId = red.ValueId });
        Assert.Equal(HttpStatusCode.OK, assign.StatusCode);

        var assignments = await client.GetFromJsonAsync<List<ProductAttributeAssignmentDto>>(
            $"/api/v1/products/{productCode}/attributes");
        var assignment = Assert.Single(assignments!);
        Assert.Equal(id, assignment.AttributeId);
        Assert.Equal("Red", assignment.SelectedValue);
    }

    [Fact]
    public async Task Assign_ValueFromOtherAttribute_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var productCode = await CreateProductAsync(client);
        var colorId = await CreateAttributeAsync(client, "Color", [("Red", null, 1)]);
        var sizeId = await CreateAttributeAsync(client, "Size", [("XL", null, 1)]);

        var sizeDetail = await client.GetFromJsonAsync<ProductAttributeDetailDto>($"/api/v1/product-attributes/{sizeId}");
        var xl = sizeDetail!.Values.Single();

        var assign = await client.PutAsJsonAsync($"/api/v1/products/{productCode}/attributes/{colorId}",
            new { SelectedValueId = xl.ValueId });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, assign.StatusCode);
    }

    [Fact]
    public async Task Delete_AttributeWithAssignments_Returns422_ThenUnassignAllowsDelete()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var productCode = await CreateProductAsync(client);
        var id = await CreateAttributeAsync(client, "Lockable", []);

        await client.PutAsJsonAsync($"/api/v1/products/{productCode}/attributes/{id}", new { });

        var blocked = await client.DeleteAsync($"/api/v1/product-attributes/{id}");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, blocked.StatusCode);

        var unassign = await client.DeleteAsync($"/api/v1/products/{productCode}/attributes/{id}");
        Assert.Equal(HttpStatusCode.NoContent, unassign.StatusCode);

        var allowed = await client.DeleteAsync($"/api/v1/product-attributes/{id}");
        Assert.Equal(HttpStatusCode.NoContent, allowed.StatusCode);

        var gone = await client.GetAsync($"/api/v1/product-attributes/{id}");
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [Fact]
    public async Task Assign_ToUnknownProduct_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var id = await CreateAttributeAsync(client, "Orphan", []);

        var assign = await client.PutAsJsonAsync($"/api/v1/products/NO-SUCH-PRODUCT/attributes/{id}", new { });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, assign.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private static async Task<int> CreateAttributeAsync(
        HttpClient client,
        string name,
        (string Value, string? GroupName, int SortOrder)[] values,
        string? code = null)
    {
        var response = await client.PostAsJsonAsync("/api/v1/product-attributes", new
        {
            Code = code ?? UniqueCode("ATT"),
            Name = name,
            Values = values.Select(v => new { v.Value, v.GroupName, v.SortOrder }).ToArray(),
        });
        Assert.True(response.IsSuccessStatusCode, $"Attribute create failed: {await response.Content.ReadAsStringAsync()}");
        var created = await response.Content.ReadFromJsonAsync<CreatedResult>();
        return created!.AttributeId;
    }

    private static async Task<string> CreateProductAsync(HttpClient client)
    {
        var uomCode = UniqueCode("EA")[..8];
        var uomResp = await client.PostAsJsonAsync("/api/v1/uom",
            new { Code = uomCode, Name = $"Each {uomCode}", Group = "Quantity" });
        Assert.True(uomResp.IsSuccessStatusCode, $"UoM create failed: {await uomResp.Content.ReadAsStringAsync()}");

        var productCode = UniqueCode("PRD");
        var prodResp = await client.PostAsJsonAsync("/api/v1/products",
            new { Code = productCode, Name = $"Product {productCode}", BaseUoMCode = uomCode });
        Assert.True(prodResp.IsSuccessStatusCode, $"Product create failed: {await prodResp.Content.ReadAsStringAsync()}");
        return productCode;
    }

    private record CreatedResult(int AttributeId);
}
