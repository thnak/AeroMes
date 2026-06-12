using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AeroMes.Application.Master.Products.Queries.GetProducts;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class ProductItemMasterTests(AeroMesWebFactory factory)
{
    // Matches the API's JsonStringEnumConverter(CamelCase) setting
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_WithProcurementAndFormula_PersistsNewFields()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (productCode, uomCode) = await CreateProductAsync(client);

        var response = await client.PutAsJsonAsync($"/api/v1/products/{productCode}",
            BuildUpdateRequest(uomCode, new Dictionary<string, object?>
            {
                ["FixedPurchasePrice"] = 125.5m,
                ["TechnicalStandard"] = "ISO 9001 / ASTM B209",
                ["QuantityFormula"] = "[Height]*[Width]*[Qty]",
            }));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var detail = await client.GetFromJsonAsync<ProductDetailDto>($"/api/v1/products/{productCode}", JsonOpts);
        Assert.NotNull(detail);
        Assert.Equal(125.5m, detail.FixedPurchasePrice);
        Assert.Equal("ISO 9001 / ASTM B209", detail.TechnicalStandard);
        Assert.Equal("[Height]*[Width]*[Qty]", detail.QuantityFormula);
    }

    [Fact]
    public async Task Update_WithUnknownFormulaVariable_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (productCode, uomCode) = await CreateProductAsync(client);

        var response = await client.PutAsJsonAsync($"/api/v1/products/{productCode}",
            BuildUpdateRequest(uomCode, new Dictionary<string, object?>
            {
                ["QuantityFormula"] = "[Diameter]*[Qty]",
            }));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task UoMConversion_AddUpdateRemove_RoundTrips()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (productCode, _) = await CreateProductAsync(client);
        var kgCode = await CreateUoMAsync(client, "Weight");

        var add = await client.PostAsJsonAsync($"/api/v1/products/{productCode}/uom-conversions",
            new { UoMCode = kgCode, ToBaseFactor = 100m, Notes = "1 tạ = 100 kg" });
        Assert.Equal(HttpStatusCode.Created, add.StatusCode);
        var created = await add.Content.ReadFromJsonAsync<ConversionCreatedResult>();

        var update = await client.PutAsJsonAsync(
            $"/api/v1/products/{productCode}/uom-conversions/{created!.ConversionId}",
            new { ToBaseFactor = 50m, Notes = "adjusted" });
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var detail = await client.GetFromJsonAsync<ProductDetailDto>($"/api/v1/products/{productCode}", JsonOpts);
        var conversion = Assert.Single(detail!.UoMConversions);
        Assert.Equal(kgCode, conversion.UoMCode);
        Assert.Equal(50m, conversion.ToBaseFactor);

        var remove = await client.DeleteAsync($"/api/v1/products/{productCode}/uom-conversions/{created.ConversionId}");
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);

        var after = await client.GetFromJsonAsync<ProductDetailDto>($"/api/v1/products/{productCode}", JsonOpts);
        Assert.Empty(after!.UoMConversions);
    }

    [Fact]
    public async Task UoMConversion_DuplicateUoM_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (productCode, _) = await CreateProductAsync(client);
        var altCode = await CreateUoMAsync(client, "Quantity");

        var first = await client.PostAsJsonAsync($"/api/v1/products/{productCode}/uom-conversions",
            new { UoMCode = altCode, ToBaseFactor = 12m });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var dup = await client.PostAsJsonAsync($"/api/v1/products/{productCode}/uom-conversions",
            new { UoMCode = altCode, ToBaseFactor = 24m });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, dup.StatusCode);
    }

    [Fact]
    public async Task UoMConversion_ForBaseUoM_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (productCode, uomCode) = await CreateProductAsync(client);

        var response = await client.PostAsJsonAsync($"/api/v1/products/{productCode}/uom-conversions",
            new { UoMCode = uomCode, ToBaseFactor = 1m });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task UoMConversion_NonPositiveFactor_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (productCode, _) = await CreateProductAsync(client);
        var altCode = await CreateUoMAsync(client, "Quantity");

        var response = await client.PostAsJsonAsync($"/api/v1/products/{productCode}/uom-conversions",
            new { UoMCode = altCode, ToBaseFactor = 0m });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ProductReferencedByBom_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (parentCode, _) = await CreateProductAsync(client);
        var (childCode, _) = await CreateProductAsync(client);

        var bom = await client.PostAsJsonAsync("/api/v1/bom-items",
            new { ParentProductCode = parentCode, ChildProductCode = childCode, RequiredQty = 2m });
        Assert.Equal(HttpStatusCode.Created, bom.StatusCode);

        var response = await client.DeleteAsync($"/api/v1/products/{parentCode}");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UnreferencedProduct_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (productCode, _) = await CreateProductAsync(client);

        var response = await client.DeleteAsync($"/api/v1/products/{productCode}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreateBomItem_WithInactiveChild_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (parentCode, _) = await CreateProductAsync(client);
        var (childCode, _) = await CreateProductAsync(client);

        var lifecycle = await client.PutAsJsonAsync($"/api/v1/products/{childCode}/lifecycle",
            new { Status = "Discontinued" });
        Assert.Equal(HttpStatusCode.NoContent, lifecycle.StatusCode);

        var response = await client.PostAsJsonAsync("/api/v1/bom-items",
            new { ParentProductCode = parentCode, ChildProductCode = childCode, RequiredQty = 1m });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private static async Task<string> CreateUoMAsync(HttpClient client, string group)
    {
        var uomCode = UniqueCode("UM")[..8];
        var response = await client.PostAsJsonAsync("/api/v1/uom",
            new { Code = uomCode, Name = $"Unit {uomCode}", Group = group });
        Assert.True(response.IsSuccessStatusCode, $"UoM create failed: {await response.Content.ReadAsStringAsync()}");
        return uomCode;
    }

    private static async Task<(string ProductCode, string UoMCode)> CreateProductAsync(HttpClient client)
    {
        var uomCode = await CreateUoMAsync(client, "Quantity");
        var productCode = UniqueCode("PRD");
        var response = await client.PostAsJsonAsync("/api/v1/products",
            new { Code = productCode, Name = $"Product {productCode}", BaseUoMCode = uomCode });
        Assert.True(response.IsSuccessStatusCode, $"Product create failed: {await response.Content.ReadAsStringAsync()}");
        return (productCode, uomCode);
    }

    private static Dictionary<string, object?> BuildUpdateRequest(string uomCode, Dictionary<string, object?> overrides)
    {
        var request = new Dictionary<string, object?>
        {
            ["Name"] = "Updated Product",
            ["BaseUoMCode"] = uomCode,
            ["PurchaseUoMCode"] = null,
            ["PurchaseToBaseQty"] = 1m,
            ["ItemType"] = "FG",
            ["CategoryId"] = null,
            ["BarcodePattern"] = null,
            ["LotControlled"] = false,
            ["SerialControlled"] = false,
            ["ShelfLifeDays"] = null,
            ["ReorderPoint"] = null,
            ["SafetyStock"] = null,
            ["LeadTimeDays"] = null,
            ["ProcurementType"] = null,
            ["EffectiveFrom"] = null,
            ["EffectiveTo"] = null,
            ["CustomerPartNo"] = null,
            ["DrawingNo"] = null,
            ["Revision"] = null,
            ["NetWeight"] = null,
            ["GrossWeight"] = null,
            ["Length"] = null,
            ["Width"] = null,
            ["Height"] = null,
            ["ImageUrl"] = null,
            ["ThumbnailUrl"] = null,
        };
        foreach (var (key, value) in overrides)
            request[key] = value;
        return request;
    }

    private record ConversionCreatedResult(int ConversionId);
}
