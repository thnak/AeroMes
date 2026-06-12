using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Master.Molds.Commands.RecordMoldShots;
using AeroMes.Application.Master.Molds.Queries.GetMoldByCode;
using AeroMes.Application.Master.Molds.Queries.GetMolds;
using AeroMes.Application.Master.Molds.Queries.GetMoldsDueForPm;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class MoldTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task Register_AddProduct_Assign_Unassign_RoundTrip()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var moldCode = UniqueCode("MOLD");
        var productCode = await CreateProductAsync(client);
        var machineCode = await CreateMachineAsync(client);

        var register = await client.PostAsJsonAsync("/api/v1/molds", new
        {
            Code = moldCode,
            Name = "Khuôn ép test",
            MoldType = "Injection",
            Material = "P20",
            Cavities = 4,
            MaxShots = 500_000L,
            PmIntervalShots = 50_000,
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        // Assigning before any product mapping is declared must fail.
        var early = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/assign",
            new { MachineCode = machineCode });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, early.StatusCode);

        var addProduct = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/products",
            new { ProductCode = productCode, IsDefault = true, CycleTimeSeconds = 32.5 });
        Assert.Equal(HttpStatusCode.Created, addProduct.StatusCode);

        var assign = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/assign",
            new { MachineCode = machineCode });
        Assert.Equal(HttpStatusCode.NoContent, assign.StatusCode);

        // Already mounted — assigning to another machine must fail.
        var second = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/assign",
            new { MachineCode = machineCode });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, second.StatusCode);

        var detail = await client.GetFromJsonAsync<MoldDetailDto>($"/api/v1/molds/{moldCode}");
        Assert.Equal(machineCode, detail!.CurrentMachineCode);
        Assert.Equal("Injection", detail.MoldType);
        Assert.Equal(4, detail.Cavities);
        var mapping = Assert.Single(detail.ProductMappings);
        Assert.Equal(productCode, mapping.ProductCode);
        Assert.True(mapping.IsDefault);

        var unassign = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/unassign", new { });
        Assert.Equal(HttpStatusCode.NoContent, unassign.StatusCode);

        detail = await client.GetFromJsonAsync<MoldDetailDto>($"/api/v1/molds/{moldCode}");
        Assert.Null(detail!.CurrentMachineCode);
    }

    [Fact]
    public async Task Register_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var moldCode = UniqueCode("MOLD");

        var first = await client.PostAsJsonAsync("/api/v1/molds", MinimalMold(moldCode));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await client.PostAsJsonAsync("/api/v1/molds", MinimalMold(moldCode));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicate.StatusCode);
    }

    [Fact]
    public async Task RecordShots_PmDue_AndMaintenanceResetsCycle()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var moldCode = UniqueCode("MOLD");
        await client.PostAsJsonAsync("/api/v1/molds", new
        {
            Code = moldCode,
            Name = "Khuôn PM test",
            MoldType = "Stamping",
            MaxShots = 1_000_000L,
            PmIntervalShots = 100,
        });

        var shots = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/shots", new { Shots = 120L });
        Assert.Equal(HttpStatusCode.OK, shots.StatusCode);
        var result = await shots.Content.ReadFromJsonAsync<RecordMoldShotsResult>();
        Assert.Equal(120, result!.CurrentShots);
        Assert.True(result.PmDue);
        Assert.False(result.NearingEndOfLife);

        var due = await client.GetFromJsonAsync<List<MoldPmDueDto>>("/api/v1/molds/due-for-pm");
        var entry = Assert.Single(due!, x => x.MoldCode == moldCode);
        Assert.True(entry.IsOverdue);

        // PM flow: send → recording shots is blocked → complete resets the cycle.
        var send = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/maintenance/send",
            new { MaintenanceType = "Pm" });
        Assert.Equal(HttpStatusCode.NoContent, send.StatusCode);

        var blocked = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/shots", new { Shots = 1L });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, blocked.StatusCode);

        var complete = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/maintenance/complete", new
        {
            MaintenanceType = "Pm",
            StartDate = DateTime.UtcNow.AddHours(-2),
            EndDate = DateTime.UtcNow,
            TechnicianId = "tech-01",
            Description = "Bảo trì định kỳ",
        });
        Assert.Equal(HttpStatusCode.Created, complete.StatusCode);

        var detail = await client.GetFromJsonAsync<MoldDetailDto>($"/api/v1/molds/{moldCode}");
        Assert.Equal("Active", detail!.Status);
        Assert.Equal(120, detail.ShotsAtLastPm);
        Assert.Equal(0, detail.ShotsSinceLastPm);
        Assert.False(detail.PmDue);
        var log = Assert.Single(detail.MaintenanceHistory);
        Assert.Equal("Pm", log.MaintenanceType);
        Assert.Equal(120, log.ShotsAtEvent);

        var dueAfter = await client.GetFromJsonAsync<List<MoldPmDueDto>>("/api/v1/molds/due-for-pm");
        Assert.DoesNotContain(dueAfter!, x => x.MoldCode == moldCode);
    }

    [Fact]
    public async Task RecordShots_NearingEndOfLife_SetsFlag()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var moldCode = UniqueCode("MOLD");
        await client.PostAsJsonAsync("/api/v1/molds", new
        {
            Code = moldCode,
            Name = "Khuôn EOL test",
            MoldType = "DieCast",
            MaxShots = 1000L,
            PmIntervalShots = 100_000,
        });

        var shots = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/shots", new { Shots = 960L });
        var result = await shots.Content.ReadFromJsonAsync<RecordMoldShotsResult>();
        Assert.True(result!.NearingEndOfLife);
        Assert.False(result.PmDue);

        var detail = await client.GetFromJsonAsync<MoldDetailDto>($"/api/v1/molds/{moldCode}");
        Assert.Equal(96.0m, detail!.ShotUtilizationPercent);
    }

    [Fact]
    public async Task Scrap_BlocksUpdates_AndExcludesFromActiveList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var moldCode = UniqueCode("MOLD");
        await client.PostAsJsonAsync("/api/v1/molds", MinimalMold(moldCode));

        var scrap = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/scrap", new { });
        Assert.Equal(HttpStatusCode.NoContent, scrap.StatusCode);

        var detail = await client.GetFromJsonAsync<MoldDetailDto>($"/api/v1/molds/{moldCode}");
        Assert.Equal("Scrapped", detail!.Status);
        Assert.False(detail.IsActive);

        var active = await client.GetFromJsonAsync<List<MoldDto>>(
            $"/api/v1/molds?activeOnly=true&search={moldCode}");
        Assert.Empty(active!);

        // Scrapped molds are frozen.
        var update = await client.PutAsJsonAsync($"/api/v1/molds/{moldCode}", new
        {
            Name = "Renamed",
            MoldType = "Injection",
            MaxShots = 1000L,
            PmIntervalShots = 100,
            Cavities = 1,
            IsActive = true,
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, update.StatusCode);

        var rescrap = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/scrap", new { });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rescrap.StatusCode);
    }

    [Fact]
    public async Task Assign_MountedMold_CannotBeSentToMaintenanceOrScrapped()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var moldCode = UniqueCode("MOLD");
        var productCode = await CreateProductAsync(client);
        var machineCode = await CreateMachineAsync(client);

        await client.PostAsJsonAsync("/api/v1/molds", MinimalMold(moldCode));
        await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/products",
            new { ProductCode = productCode });
        await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/assign",
            new { MachineCode = machineCode });

        var send = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/maintenance/send",
            new { MaintenanceType = "Repair" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, send.StatusCode);

        var scrap = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/scrap", new { });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, scrap.StatusCode);

        var delete = await client.DeleteAsync($"/api/v1/molds/{moldCode}");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, delete.StatusCode);
    }

    [Fact]
    public async Task Assign_UnknownMachine_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var moldCode = UniqueCode("MOLD");
        await client.PostAsJsonAsync("/api/v1/molds", MinimalMold(moldCode));

        var assign = await client.PostAsJsonAsync($"/api/v1/molds/{moldCode}/assign",
            new { MachineCode = "NO-SUCH-MACHINE" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, assign.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private static object MinimalMold(string code) => new
    {
        Code = code,
        Name = $"Mold {code}",
        MoldType = "Injection",
        MaxShots = 100_000L,
        PmIntervalShots = 10_000,
    };

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

    private static async Task<string> CreateMachineAsync(HttpClient client)
    {
        var wcResp = await client.PostAsJsonAsync("/api/v1/work-centers",
            new { Code = UniqueCode("WC"), Name = "WC for molds" });
        Assert.True(wcResp.IsSuccessStatusCode, $"WorkCenter create failed: {await wcResp.Content.ReadAsStringAsync()}");
        var wc = await wcResp.Content.ReadFromJsonAsync<WorkCenterCreated>();

        var machineCode = UniqueCode("MCH");
        var mResp = await client.PostAsJsonAsync("/api/v1/machines",
            new { Code = machineCode, Name = $"Machine {machineCode}", WorkCenterId = wc!.WorkCenterId });
        Assert.True(mResp.IsSuccessStatusCode, $"Machine create failed: {await mResp.Content.ReadAsStringAsync()}");
        return machineCode;
    }

    private record WorkCenterCreated(int WorkCenterId);
}
