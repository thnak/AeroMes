using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AeroMes.Application.Master.Products.Queries.GetProductSpecifications;
using AeroMes.Application.Master.Products.Queries.GetProductVariants;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class ProductVariantSpecTests(AeroMesWebFactory factory)
{
    // Matches the API's JsonStringEnumConverter(CamelCase) setting
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    // ── Variant model ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateVariant_UnderVariantMode_CreatesLinkedProduct()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        await SetManagementTypeAsync(client, "VariantCode");
        var masterCode = await CreateProductAsync(client);
        var variantCode = UniqueCode("VAR");
        try
        {
            var response = await client.PostAsJsonAsync($"/api/v1/products/{masterCode}/variants",
                new { Code = variantCode, Name = "Red / XL" });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var variants = await client.GetFromJsonAsync<List<ProductVariantDto>>(
                $"/api/v1/products/{masterCode}/variants", JsonOpts);
            var variant = Assert.Single(variants!);
            Assert.Equal(variantCode, variant.ProductCode);
            Assert.Equal("Red / XL", variant.ProductName);
        }
        finally
        {
            await client.DeleteAsync($"/api/v1/products/{variantCode}");
            await SetManagementTypeAsync(client, "None");
        }
    }

    [Fact]
    public async Task CreateVariant_UnderWrongMode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        await SetManagementTypeAsync(client, "None");
        var masterCode = await CreateProductAsync(client);

        var response = await client.PostAsJsonAsync($"/api/v1/products/{masterCode}/variants",
            new { Code = UniqueCode("VAR"), Name = "Blocked" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateVariant_OfAVariant_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        await SetManagementTypeAsync(client, "VariantCode");
        var masterCode = await CreateProductAsync(client);
        var variantCode = UniqueCode("VAR");
        try
        {
            var first = await client.PostAsJsonAsync($"/api/v1/products/{masterCode}/variants",
                new { Code = variantCode, Name = "Level 1" });
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            var nested = await client.PostAsJsonAsync($"/api/v1/products/{variantCode}/variants",
                new { Code = UniqueCode("VAR"), Name = "Level 2" });
            Assert.Equal(HttpStatusCode.UnprocessableEntity, nested.StatusCode);
        }
        finally
        {
            await client.DeleteAsync($"/api/v1/products/{variantCode}");
            await SetManagementTypeAsync(client, "None");
        }
    }

    [Fact]
    public async Task ChangeManagementType_WithVariantLinks_Blocked()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        await SetManagementTypeAsync(client, "VariantCode");
        var masterCode = await CreateProductAsync(client);
        var variantCode = UniqueCode("VAR");

        var created = await client.PostAsJsonAsync($"/api/v1/products/{masterCode}/variants",
            new { Code = variantCode, Name = "Lock holder" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var blocked = await TrySetManagementTypeAsync(client, "None");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, blocked.StatusCode);

        // Removing the variant unblocks the setting.
        await DeleteProductAsync(client, variantCode);
        await SetManagementTypeAsync(client, "None");
    }

    // ── Specification-code model ──────────────────────────────────────────────

    [Fact]
    public async Task Specifications_UnderSpecMode_CrudRoundTrip()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        await SetManagementTypeAsync(client, "SpecificationCode");
        try
        {
            var productCode = await CreateProductAsync(client);

            var add = await client.PostAsJsonAsync($"/api/v1/products/{productCode}/specifications",
                new { SpecCode = "D8MM", Description = "Đường kính 8 mm" });
            Assert.Equal(HttpStatusCode.Created, add.StatusCode);
            var created = await add.Content.ReadFromJsonAsync<SpecCreatedResult>();

            var dup = await client.PostAsJsonAsync($"/api/v1/products/{productCode}/specifications",
                new { SpecCode = "d8mm" });
            Assert.Equal(HttpStatusCode.UnprocessableEntity, dup.StatusCode);

            var update = await client.PutAsJsonAsync(
                $"/api/v1/products/{productCode}/specifications/{created!.SpecificationId}",
                new { Description = "Ø8 mm (rev B)", IsActive = true });
            Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

            var specs = await client.GetFromJsonAsync<List<ProductSpecificationDto>>(
                $"/api/v1/products/{productCode}/specifications");
            var spec = Assert.Single(specs!);
            Assert.Equal("D8MM", spec.SpecCode);
            Assert.Equal("Ø8 mm (rev B)", spec.Description);

            var remove = await client.DeleteAsync(
                $"/api/v1/products/{productCode}/specifications/{created.SpecificationId}");
            Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);
        }
        finally
        {
            await SetManagementTypeAsync(client, "None");
        }
    }

    [Fact]
    public async Task AddSpecification_UnderWrongMode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        await SetManagementTypeAsync(client, "None");
        var productCode = await CreateProductAsync(client);

        var response = await client.PostAsJsonAsync($"/api/v1/products/{productCode}/specifications",
            new { SpecCode = "D10MM" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private static async Task<HttpResponseMessage> TrySetManagementTypeAsync(HttpClient client, string mode)
    {
        var options = await client.GetFromJsonAsync<JsonObject>("/api/v1/settings/system-options");
        options!["materialManagementType"] = mode;
        return await client.PutAsJsonAsync("/api/v1/settings/system-options", options);
    }

    private static async Task SetManagementTypeAsync(HttpClient client, string mode)
    {
        var response = await TrySetManagementTypeAsync(client, mode);
        Assert.True(response.IsSuccessStatusCode,
            $"Setting MaterialManagementType={mode} failed: {await response.Content.ReadAsStringAsync()}");
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

    private static async Task DeleteProductAsync(HttpClient client, string productCode)
    {
        var response = await client.DeleteAsync($"/api/v1/products/{productCode}");
        Assert.True(response.IsSuccessStatusCode,
            $"Product delete failed: {await response.Content.ReadAsStringAsync()}");
    }

    private record SpecCreatedResult(int SpecificationId);
}
