using System.Net;
using System.Net.Http.Json;

namespace AeroMes.IntegrationTests.Iot;

/// <summary>
/// Integration tests for IoT Signal configuration and Adapter management — closes #262.
/// </summary>
[Collection("Integration")]
public class IotSignalTests(AeroMesWebFactory factory)
{
    // ── Auth guard ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSignalTags_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/iot/tags");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetAdapters_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/iot/adapters?machineCode=MCH-001");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Signal Tags ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetSignalTags_Authenticated_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/iot/tags");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task CreateSignalTag_ValidTag_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateSignalTagAsync(client);
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task CreateSignalTag_EmptyKey_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/iot/tags", new
        {
            key = "",
            displayName = "Speed",
            category = "PRODUCTION",
            dataType = "FLOAT",
            defaultUnit = (string?)null,
            typicalMin = (double?)null,
            typicalMax = (double?)null,
            description = (string?)null
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task UpdateSignalTag_AfterCreate_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var key = $"sig.{Guid.NewGuid():N}"[..18];
        await client.PostAsJsonAsync("/api/v1/iot/tags", new
        {
            key,
            displayName = "Speed",
            category = "PRODUCTION",
            dataType = "FLOAT",
            defaultUnit = "RPM",
            typicalMin = (double?)0,
            typicalMax = (double?)3000,
            description = (string?)null
        });

        var resp = await client.PutAsJsonAsync($"/api/v1/iot/tags/{Uri.EscapeDataString(key)}", new
        {
            displayName = "Speed Updated",
            category = "PRODUCTION",
            dataType = "FLOAT",
            defaultUnit = "RPM",
            typicalMin = (double?)0,
            typicalMax = (double?)5000,
            description = "Updated description"
        });
        Assert.True(resp.IsSuccessStatusCode, $"Update tag failed: {await resp.Content.ReadAsStringAsync()}");
    }

    // ── Adapters ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAdapter_Webhook_Returns201WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await CreateAdapterAsync(client, "MCH-IOT-001", "Webhook");
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var result = await resp.Content.ReadFromJsonAsync<AdapterCreatedResult>();
        Assert.NotNull(result);
        Assert.True(result!.AdapterId > 0);
    }

    [Fact]
    public async Task CreateAdapter_EmptyMachineCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/iot/adapters", new
        {
            machineCode = "",
            adapterType = "Webhook",
            configJson = "{}"
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task GetAdapters_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/iot/adapters?machineCode=MCH-IOT-001");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetAdapterDetail_AfterCreate_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await CreateAdapterAsync(client, "MCH-IOT-002", "Webhook");
        var result = await createResp.Content.ReadFromJsonAsync<AdapterCreatedResult>();

        var resp = await client.GetAsync($"/api/v1/iot/adapters/{result!.AdapterId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Signals on adapter ────────────────────────────────────────────────

    [Fact]
    public async Task AddSignal_ToAdapter_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        // Create signal tag first
        var tagKey = $"sig.{Guid.NewGuid():N}"[..18];
        await CreateSignalTagAsync(client, tagKey);

        var createAdapterResp = await CreateAdapterAsync(client, "MCH-IOT-003", "Webhook");
        var adapter = await createAdapterResp.Content.ReadFromJsonAsync<AdapterCreatedResult>();

        var resp = await client.PostAsJsonAsync($"/api/v1/iot/adapters/{adapter!.AdapterId}/signals", new
        {
            tagKey,
            displayName = "Machine Speed",
            sourceAddress = "/DeviceSpeed",
            scaleFactor = 1.0,
            offset = 0.0,
            unit = "RPM"
        });
        Assert.True(resp.IsSuccessStatusCode, $"Add signal failed: {await resp.Content.ReadAsStringAsync()}");
    }

    // ── State Rules ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetStateRules_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/iot/state-rules?machineCode=MCH-IOT-001");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task CreateStateRule_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var tagKey = $"sig.{Guid.NewGuid():N}"[..18];
        await CreateSignalTagAsync(client, tagKey);

        var resp = await client.PostAsJsonAsync("/api/v1/iot/state-rules", new
        {
            machineCode = "MCH-IOT-004",
            priority = 10,
            targetState = "Running",
            signalTagKey = tagKey,
            @operator = "GT",
            threshold = 0.0,
            description = "Speed > 0 means running"
        });
        Assert.True(resp.IsSuccessStatusCode, $"Create state rule failed: {await resp.Content.ReadAsStringAsync()}");
    }

    // ── Adapter health ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAdapterHealth_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/iot/adapters/health");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Pipeline stats ────────────────────────────────────────────────────

    [Fact]
    public async Task GetPipelineStats_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/iot/pipeline/stats");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private Task<HttpResponseMessage> CreateSignalTagAsync(HttpClient client)
        => CreateSignalTagAsync(client, $"sig.{Guid.NewGuid():N}"[..18]);

    private static Task<HttpResponseMessage> CreateSignalTagAsync(HttpClient client, string key)
        => client.PostAsJsonAsync("/api/v1/iot/tags", new
        {
            key,
            displayName = "Integration Test Signal",
            category = "PRODUCTION",
            dataType = "FLOAT",
            defaultUnit = "RPM",
            typicalMin = (double?)0,
            typicalMax = (double?)3000,
            description = (string?)null
        });

    private static Task<HttpResponseMessage> CreateAdapterAsync(
        HttpClient client, string machineCode, string adapterType)
        => client.PostAsJsonAsync("/api/v1/iot/adapters", new
        {
            machineCode,
            adapterType,
            configJson = "{\"timeout\": 5000}"
        });
}

file record AdapterCreatedResult(int AdapterId);
