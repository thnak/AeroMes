using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Common;
using AeroMes.Application.Downtime.Queries.GetDowntimeLogs;
using AeroMes.Domain.Master;
using AeroMes.Domain.Production;

namespace AeroMes.IntegrationTests.Production;

[Collection("Integration")]
public class DowntimeLogTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/downtime");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/downtime/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Authenticated_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/downtime");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DowntimeLogDto>>>();
        Assert.NotNull(body?.Data);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/downtime/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AfterSeed_ReturnsSeededEntry()
    {
        var (machineCode, reasonCode, logId) = await SeedDowntimeLogAsync();

        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync($"/api/v1/downtime?machineCode={machineCode}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DowntimeLogDto>>>();
        Assert.NotNull(body?.Data);
        Assert.Contains(body.Data, x => x.DowntimeLogID == logId && x.IsOpen);
    }

    [Fact]
    public async Task GetById_ExistingLog_Returns200WithDetails()
    {
        var (machineCode, reasonCode, logId) = await SeedDowntimeLogAsync();

        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync($"/api/v1/downtime/{logId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<DowntimeLogDto>>();
        Assert.NotNull(body?.Data);
        Assert.Equal(machineCode, body.Data.MachineCode);
        Assert.Equal(reasonCode, body.Data.ReasonCode);
        Assert.True(body.Data.IsOpen);
    }

    [Fact]
    public async Task GetAll_FilterByIsOpen_ReturnsOnlyOpen()
    {
        var (machineCode, _, openLogId) = await SeedDowntimeLogAsync();
        var (_, __, closedLogId) = await SeedClosedDowntimeLogAsync(machineCode);

        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync($"/api/v1/downtime?machineCode={machineCode}&isOpen=true");
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DowntimeLogDto>>>();

        Assert.NotNull(body?.Data);
        var mine = body.Data.Where(x => x.MachineCode == machineCode).ToList();
        Assert.Contains(mine, x => x.DowntimeLogID == openLogId);
        Assert.DoesNotContain(mine, x => x.DowntimeLogID == closedLogId);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<(string machineCode, string reasonCode, long logId)> SeedDowntimeLogAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var wcCode = $"WC-{suffix}";    // 11 chars — well within 50
        var mCode = $"MCH-{suffix}";    // 12 chars — well within 50
        var rcCode = $"RC-{suffix}";    // 11 chars — well within 30

        using var db = factory.CreateDbContext();

        // Seed WorkCenter
        var wc = WorkCenter.Create(wcCode, "Test WC");
        db.WorkCenters.Add(wc);
        await db.SaveChangesAsync();

        // Seed Machine + DowntimeReasonCode in same SaveChanges
        var machine = Machine.Create(mCode, "Test MCH", wc.WorkCenterID);
        var rc = DowntimeReasonCode.Create(rcCode, "Test Reason", DowntimeCategory.Unplanned);
        db.Machines.Add(machine);
        db.DowntimeReasonCodes.Add(rc);
        await db.SaveChangesAsync();

        // Seed DowntimeLog in same context
        var log = DowntimeLog.Create(mCode, rcCode, "Reason Name", DateTime.UtcNow.AddMinutes(-10), "op-test");
        db.DowntimeLogs.Add(log);
        await db.SaveChangesAsync();

        return (mCode, rcCode, log.DowntimeLogID);
    }

    private async Task<(string machineCode, string reasonCode, long logId)> SeedClosedDowntimeLogAsync(string machineCode)
    {
        using var db = factory.CreateDbContext();

        // Find existing reason code for this machine, or create a new one
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var rcCode = $"RC{suffix}"[..10];
        var rc = DowntimeReasonCode.Create(rcCode, "Closed Reason", DowntimeCategory.Planned);
        db.DowntimeReasonCodes.Add(rc);
        await db.SaveChangesAsync();

        var log = DowntimeLog.Create(machineCode, rcCode, null, DateTime.UtcNow.AddHours(-2), "op-test2");
        log.End(DateTime.UtcNow.AddHours(-1));
        db.DowntimeLogs.Add(log);
        await db.SaveChangesAsync();

        return (machineCode, rcCode, log.DowntimeLogID);
    }
}
