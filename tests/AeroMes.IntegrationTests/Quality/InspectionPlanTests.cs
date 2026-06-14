using System.Net;
using System.Net.Http.Json;

namespace AeroMes.IntegrationTests.Quality;

/// <summary>
/// Integration tests for Quality Inspection Plans — closes #259.
/// </summary>
[Collection("Integration")]
public class InspectionPlanTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetInspectionPlans_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/quality/inspection-plans");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Create inspection plan ─────────────────────────────────────────────

    [Fact]
    public async Task CreateInspectionPlan_ValidPlan_Returns201WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreatePlanAsync(client, "DIMENSIONAL");
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var result = await resp.Content.ReadFromJsonAsync<InspectionPlanCreatedResult>();
        Assert.NotNull(result);
        Assert.True(result!.PlanId > 0);
    }

    [Fact]
    public async Task CreateInspectionPlan_InvalidSamplingMethod_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/quality/inspection-plans", new
        {
            code = $"PLAN-{Guid.NewGuid():N}"[..12],
            name = "Test Plan",
            routingStepId = 1,
            productCode = (string?)null,
            samplingMethod = "RANDOM_BAD",
            sampleSize = (int?)null,
            acceptNumber = (int?)null,
            rejectNumber = (int?)null,
            inspectionType = "DIMENSIONAL",
            notes = (string?)null
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task CreateInspectionPlan_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = $"PLAN-{Guid.NewGuid():N}"[..12];
        await CreatePlanWithCodeAsync(client, code, "VISUAL");
        var dupResp = await CreatePlanWithCodeAsync(client, code, "VISUAL");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, dupResp.StatusCode);
    }

    // ── Get plans ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetInspectionPlans_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/quality/inspection-plans");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetInspectionPlanDetail_AfterCreate_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreatePlanAsync(client, "FUNCTIONAL");
        var result = await createResp.Content.ReadFromJsonAsync<InspectionPlanCreatedResult>();

        var resp = await client.GetAsync($"/api/v1/quality/inspection-plans/{result!.PlanId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetInspectionPlanDetail_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/quality/inspection-plans/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Activate / deactivate ──────────────────────────────────────────────

    [Fact]
    public async Task ActivateInspectionPlan_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreatePlanAsync(client, "COMBINED");
        var result = await createResp.Content.ReadFromJsonAsync<InspectionPlanCreatedResult>();

        var resp = await client.PostAsync($"/api/v1/quality/inspection-plans/{result!.PlanId}/activate", null);
        Assert.True(resp.IsSuccessStatusCode, $"Activate failed: {await resp.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task DeactivateInspectionPlan_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreatePlanAsync(client, "DIMENSIONAL");
        var result = await createResp.Content.ReadFromJsonAsync<InspectionPlanCreatedResult>();
        await client.PostAsync($"/api/v1/quality/inspection-plans/{result!.PlanId}/activate", null);

        var resp = await client.PostAsync($"/api/v1/quality/inspection-plans/{result.PlanId}/deactivate", null);
        Assert.True(resp.IsSuccessStatusCode, $"Deactivate failed: {await resp.Content.ReadAsStringAsync()}");
    }

    // ── Characteristics ────────────────────────────────────────────────────

    [Fact]
    public async Task AddCharacteristic_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreatePlanAsync(client, "DIMENSIONAL");
        var plan = await createResp.Content.ReadFromJsonAsync<InspectionPlanCreatedResult>();

        var resp = await client.PostAsJsonAsync($"/api/v1/quality/inspection-plans/{plan!.PlanId}/characteristics", new
        {
            characteristicName = "Outer Diameter",
            characteristicType = "MEASUREMENT",
            uom = "mm",
            lsl = 9.8m,
            usl = 10.2m,
            nominal = 10.0m,
            isKeyCharacteristic = true,
            notes = (string?)null
        });
        Assert.True(resp.IsSuccessStatusCode, $"Add char failed: {await resp.Content.ReadAsStringAsync()}");
    }

    // ── Inspection Orders ──────────────────────────────────────────────────

    [Fact]
    public async Task GetInspectionOrders_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/quality/inspection-orders");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetInspectionOrderDetail_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/quality/inspection-orders/999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Task<HttpResponseMessage> CreatePlanAsync(HttpClient client, string inspectionType)
        => CreatePlanWithCodeAsync(client, $"PLAN-{Guid.NewGuid():N}"[..12], inspectionType);

    private static Task<HttpResponseMessage> CreatePlanWithCodeAsync(
        HttpClient client, string code, string inspectionType)
        => client.PostAsJsonAsync("/api/v1/quality/inspection-plans", new
        {
            code,
            name = $"Test Plan {code}",
            routingStepId = 1,
            productCode = (string?)null,
            samplingMethod = "FULL",
            sampleSize = (int?)null,
            acceptNumber = (int?)null,
            rejectNumber = (int?)null,
            inspectionType,
            notes = "Integration test plan"
        });
}

file record InspectionPlanCreatedResult(int PlanId);
