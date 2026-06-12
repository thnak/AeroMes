using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Master.Customers.Queries.GetCustomerById;
using AeroMes.Application.Master.Customers.Queries.GetCustomers;
using AeroMes.Application.Master.Customers.Queries.LookupCustomerPart;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class CustomerTests(AeroMesWebFactory factory)
{
    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/customers");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidPayload_Returns201WithCode()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("CUS");

        var response = await client.PostAsJsonAsync("/api/v1/customers",
            new { Code = code, Name = "Acme Manufacturing", CustomerType = "Oem", Country = "Vietnam" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreatedResult>();
        Assert.Equal(code, body?.CustomerCode);
    }

    [Fact]
    public async Task Create_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DUP");

        await client.PostAsJsonAsync("/api/v1/customers", new { Code = code, Name = "First", CustomerType = "Direct" });

        var response = await client.PostAsJsonAsync("/api/v1/customers", new { Code = code, Name = "Duplicate", CustomerType = "Direct" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyName_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/customers",
            new { Code = UniqueCode("NNM"), Name = "", CustomerType = "Direct" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetById_AfterCreate_ReturnsDetail()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DET");

        await client.PostAsJsonAsync("/api/v1/customers",
            new { Code = code, Name = "Detail Customer", CustomerType = "Distributor", Currency = "USD", CreditTermsDays = 45 });

        var response = await client.GetAsync($"/api/v1/customers/{code}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.Content.ReadFromJsonAsync<CustomerDetailDto>();
        Assert.NotNull(detail);
        Assert.Equal("Detail Customer", detail.CustomerName);
        Assert.Equal("Distributor", detail.CustomerType);
        Assert.Equal("USD", detail.Currency);
        Assert.Equal(45, detail.CreditTermsDays);
    }

    [Fact]
    public async Task Update_ExistingCustomer_Returns204AndPersistsChange()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("UPD");

        await client.PostAsJsonAsync("/api/v1/customers", new { Code = code, Name = "Original", CustomerType = "Direct" });

        var updateResp = await client.PutAsJsonAsync($"/api/v1/customers/{code}",
            new { Name = "Updated", CustomerType = "Oem", CreditTermsDays = 60, IsActive = true });

        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        var detail = await (await client.GetAsync($"/api/v1/customers/{code}"))
            .Content.ReadFromJsonAsync<CustomerDetailDto>();
        Assert.Equal("Updated", detail!.CustomerName);
        Assert.Equal("Oem", detail.CustomerType);
        Assert.Equal(60, detail.CreditTermsDays);
    }

    [Fact]
    public async Task Delete_ExistingCustomer_Returns204AndHidesFromList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DEL");

        await client.PostAsJsonAsync("/api/v1/customers", new { Code = code, Name = "To Delete", CustomerType = "Direct" });

        var deleteResp = await client.DeleteAsync($"/api/v1/customers/{code}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        var items = await (await client.GetAsync("/api/v1/customers?activeOnly=false"))
            .Content.ReadFromJsonAsync<List<CustomerDto>>();
        Assert.DoesNotContain(items!, x => x.CustomerCode == code);
    }

    [Fact]
    public async Task Delete_NonExistent_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.DeleteAsync("/api/v1/customers/NOTHERE-404");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddPartNumber_ThenLookup_ResolvesInternalProduct()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customerCode = UniqueCode("CPN");
        var productCode = await CreateProductAsync(client);

        await client.PostAsJsonAsync("/api/v1/customers",
            new { Code = customerCode, Name = "Part Mapping Customer", CustomerType = "Oem" });

        var addResp = await client.PostAsJsonAsync($"/api/v1/customers/{customerCode}/parts",
            new { CustomerPartNo = "ACME-001", ProductCode = productCode, Revision = "B" });
        Assert.Equal(HttpStatusCode.Created, addResp.StatusCode);

        var lookupResp = await client.GetAsync(
            $"/api/v1/customers/{customerCode}/parts/lookup?customerPartNo=ACME-001");
        Assert.Equal(HttpStatusCode.OK, lookupResp.StatusCode);

        var lookup = await lookupResp.Content.ReadFromJsonAsync<CustomerPartLookupDto>();
        Assert.NotNull(lookup);
        Assert.Equal(productCode, lookup.ProductCode);
        Assert.Equal("B", lookup.Revision);
    }

    [Fact]
    public async Task AddPartNumber_DuplicatePartNo_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customerCode = UniqueCode("CPD");
        var productCode = await CreateProductAsync(client);

        await client.PostAsJsonAsync("/api/v1/customers",
            new { Code = customerCode, Name = "Dup Part Customer", CustomerType = "Oem" });

        await client.PostAsJsonAsync($"/api/v1/customers/{customerCode}/parts",
            new { CustomerPartNo = "DUP-PN", ProductCode = productCode });

        var response = await client.PostAsJsonAsync($"/api/v1/customers/{customerCode}/parts",
            new { CustomerPartNo = "DUP-PN", ProductCode = productCode });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task AddPartNumber_UnknownProduct_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customerCode = UniqueCode("CPX");

        await client.PostAsJsonAsync("/api/v1/customers",
            new { Code = customerCode, Name = "Bad Product Customer", CustomerType = "Oem" });

        var response = await client.PostAsJsonAsync($"/api/v1/customers/{customerCode}/parts",
            new { CustomerPartNo = "GHOST-PN", ProductCode = "NO-SUCH-PRODUCT" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task SetQualitySpec_TwiceForSameProduct_UpsertsSingleSpec()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customerCode = UniqueCode("CQS");
        var productCode = await CreateProductAsync(client);

        await client.PostAsJsonAsync("/api/v1/customers",
            new { Code = customerCode, Name = "Quality Spec Customer", CustomerType = "Oem" });

        var firstResp = await client.PutAsJsonAsync($"/api/v1/customers/{customerCode}/quality-specs",
            new { ProductCode = productCode, AqlLevel = "1.0", InspectionLevel = "II" });
        Assert.Equal(HttpStatusCode.OK, firstResp.StatusCode);

        var secondResp = await client.PutAsJsonAsync($"/api/v1/customers/{customerCode}/quality-specs",
            new { ProductCode = productCode, AqlLevel = "0.65", InspectionLevel = "III", CertificateRequired = true });
        Assert.Equal(HttpStatusCode.OK, secondResp.StatusCode);

        var detail = await (await client.GetAsync($"/api/v1/customers/{customerCode}"))
            .Content.ReadFromJsonAsync<CustomerDetailDto>();
        var spec = Assert.Single(detail!.QualitySpecs);
        Assert.Equal("0.65", spec.AqlLevel);
        Assert.Equal("III", spec.InspectionLevel);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private static async Task<string> CreateProductAsync(HttpClient client)
    {
        // Products require an existing UoM; create one per test to stay independent.
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

    private record CreatedResult(string CustomerCode);
}
