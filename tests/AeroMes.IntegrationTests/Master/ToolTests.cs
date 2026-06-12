using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Master.Tools.Commands.RecordToolUsage;
using AeroMes.Application.Master.Tools.Queries.GetToolByCode;
using AeroMes.Application.Master.Tools.Queries.GetTools;
using AeroMes.Application.Master.Tools.Queries.GetToolsDueForCalibration;
using AeroMes.Application.Master.Tools.Queries.GetToolsDueForReconditioning;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class ToolTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task Register_AddOperation_Checkout_Return_RoundTrip()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var toolCode = UniqueCode("TOOL");
        var operationCode = await CreateOperationAsync(client);
        var workCenterId = await CreateWorkCenterAsync(client);

        var register = await client.PostAsJsonAsync("/api/v1/tools", new
        {
            Code = toolCode,
            Name = "Dao phay Ø12",
            ToolType = "CuttingTool",
            Specification = "Ø12 HSS End Mill 4-flute",
            MaxUsageCount = 10_000,
            PmIntervalCount = 1_000,
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var addOp = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/operations",
            new { OperationCode = operationCode, UsageCountPerOp = 1.5m });
        Assert.Equal(HttpStatusCode.Created, addOp.StatusCode);

        var checkout = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/checkout",
            new { WorkCenterId = workCenterId, CheckedOutBy = "operator-1" });
        Assert.Equal(HttpStatusCode.Created, checkout.StatusCode);

        // Already in use — a second checkout must fail.
        var again = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/checkout",
            new { WorkCenterId = workCenterId, CheckedOutBy = "operator-2" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, again.StatusCode);

        var detail = await client.GetFromJsonAsync<ToolDetailDto>($"/api/v1/tools/{toolCode}");
        Assert.Equal("InUse", detail!.Status);
        Assert.Equal(workCenterId, detail.CurrentWorkCenterId);
        var mapping = Assert.Single(detail.OperationMappings);
        Assert.Equal(operationCode, mapping.OperationCode);
        Assert.Equal(1.5m, mapping.UsageCountPerOp);

        var ret = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/return",
            new { Condition = "Good", ReturnedBy = "operator-1" });
        Assert.Equal(HttpStatusCode.NoContent, ret.StatusCode);

        detail = await client.GetFromJsonAsync<ToolDetailDto>($"/api/v1/tools/{toolCode}");
        Assert.Equal("Available", detail!.Status);
        Assert.Null(detail.CurrentWorkCenterId);
        var record = Assert.Single(detail.CheckoutHistory);
        Assert.Equal("operator-1", record.CheckedOutBy);
        Assert.NotNull(record.ReturnedAt);
        Assert.Equal("Good", record.ConditionOnReturn);
    }

    [Fact]
    public async Task Register_DuplicateCode_AndMissingCalibrationInterval_Return422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var toolCode = UniqueCode("TOOL");

        var first = await client.PostAsJsonAsync("/api/v1/tools", MinimalTool(toolCode));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await client.PostAsJsonAsync("/api/v1/tools", MinimalTool(toolCode));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicate.StatusCode);

        // RequiresCalibration without a calibration interval is rejected.
        var invalid = await client.PostAsJsonAsync("/api/v1/tools", new
        {
            Code = UniqueCode("TOOL"),
            Name = "Thước cặp",
            ToolType = "Gauge",
            RequiresCalibration = true,
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalid.StatusCode);
    }

    [Fact]
    public async Task Usage_ReconditioningDue_AndMaintenanceResetsCycle()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var toolCode = UniqueCode("TOOL");
        await client.PostAsJsonAsync("/api/v1/tools", new
        {
            Code = toolCode,
            Name = "Đồ gá PM test",
            ToolType = "Fixture",
            PmIntervalCount = 100,
        });

        var usage = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/usage", new { Count = 120 });
        Assert.Equal(HttpStatusCode.OK, usage.StatusCode);
        var result = await usage.Content.ReadFromJsonAsync<RecordToolUsageResult>();
        Assert.Equal(120, result!.CurrentUsageCount);
        Assert.True(result.ReconditioningDue);
        Assert.False(result.NearingEndOfLife); // no MaxUsageCount → durable

        var due = await client.GetFromJsonAsync<List<ToolReconditioningDueDto>>(
            "/api/v1/tools/due-reconditioning");
        var entry = Assert.Single(due!, x => x.ToolCode == toolCode);
        Assert.True(entry.IsOverdue);

        var maintenance = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/maintenance", new
        {
            MaintenanceType = "Reconditioning",
            PerformedAt = DateTime.UtcNow,
            PerformedBy = "tech-01",
        });
        Assert.Equal(HttpStatusCode.Created, maintenance.StatusCode);

        var detail = await client.GetFromJsonAsync<ToolDetailDto>($"/api/v1/tools/{toolCode}");
        Assert.Equal(120, detail!.UsageCountAtLastPm);
        Assert.Equal(0, detail.UsageSinceLastPm);
        Assert.False(detail.ReconditioningDue);

        var dueAfter = await client.GetFromJsonAsync<List<ToolReconditioningDueDto>>(
            "/api/v1/tools/due-reconditioning");
        Assert.DoesNotContain(dueAfter!, x => x.ToolCode == toolCode);
    }

    [Fact]
    public async Task Calibration_SetsNextDue_AndDueListRespectsWindow()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var toolCode = UniqueCode("TOOL");
        await client.PostAsJsonAsync("/api/v1/tools", new
        {
            Code = toolCode,
            Name = "Thước đo hiệu chuẩn",
            ToolType = "Gauge",
            RequiresCalibration = true,
            CalibrationIntervalDays = 30,
        });

        // Send to calibration and record completion — NextCalibrationDue = performedAt + 30 days.
        var send = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/service",
            new { ServiceType = "Calibration" });
        Assert.Equal(HttpStatusCode.NoContent, send.StatusCode);

        var performedAt = DateTime.UtcNow;
        await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/maintenance", new
        {
            MaintenanceType = "Calibration",
            PerformedAt = performedAt,
            PerformedBy = "lab-01",
        });

        var detail = await client.GetFromJsonAsync<ToolDetailDto>($"/api/v1/tools/{toolCode}");
        Assert.Equal("Available", detail!.Status);
        Assert.Equal(DateOnly.FromDateTime(performedAt).AddDays(30), detail.NextCalibrationDue);

        // Due in 30 days: outside the 7-day window, inside a 60-day window.
        var nearWindow = await client.GetFromJsonAsync<List<ToolCalibrationDueDto>>(
            "/api/v1/tools/due-calibration?withinDays=7");
        Assert.DoesNotContain(nearWindow!, x => x.ToolCode == toolCode);

        var wideWindow = await client.GetFromJsonAsync<List<ToolCalibrationDueDto>>(
            "/api/v1/tools/due-calibration?withinDays=60");
        var entry = Assert.Single(wideWindow!, x => x.ToolCode == toolCode);
        Assert.False(entry.IsOverdue);
    }

    [Fact]
    public async Task Return_Damaged_GoesToRepair_AndRepairRestoresAvailable()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var toolCode = UniqueCode("TOOL");
        var workCenterId = await CreateWorkCenterAsync(client);
        await client.PostAsJsonAsync("/api/v1/tools", MinimalTool(toolCode));

        await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/checkout",
            new { WorkCenterId = workCenterId, CheckedOutBy = "operator-1" });
        await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/return",
            new { Condition = "Damaged", ReturnedBy = "operator-1", Notes = "Mẻ lưỡi cắt" });

        var detail = await client.GetFromJsonAsync<ToolDetailDto>($"/api/v1/tools/{toolCode}");
        Assert.Equal("InRepair", detail!.Status);

        // While in repair the tool cannot be checked out.
        var blocked = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/checkout",
            new { WorkCenterId = workCenterId, CheckedOutBy = "operator-2" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, blocked.StatusCode);

        await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/maintenance", new
        {
            MaintenanceType = "Repair",
            PerformedAt = DateTime.UtcNow,
        });

        detail = await client.GetFromJsonAsync<ToolDetailDto>($"/api/v1/tools/{toolCode}");
        Assert.Equal("Available", detail!.Status);
    }

    [Fact]
    public async Task Scrap_BlockedWhileCheckedOut_AndScrappedToolIsFrozen()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var toolCode = UniqueCode("TOOL");
        var workCenterId = await CreateWorkCenterAsync(client);
        await client.PostAsJsonAsync("/api/v1/tools", MinimalTool(toolCode));

        await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/checkout",
            new { WorkCenterId = workCenterId, CheckedOutBy = "operator-1" });

        var blocked = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/scrap", new { });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, blocked.StatusCode);

        await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/return",
            new { Condition = "Worn", ReturnedBy = "operator-1" });

        var scrap = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/scrap", new { });
        Assert.Equal(HttpStatusCode.NoContent, scrap.StatusCode);

        var detail = await client.GetFromJsonAsync<ToolDetailDto>($"/api/v1/tools/{toolCode}");
        Assert.Equal("Scrapped", detail!.Status);
        Assert.False(detail.IsActive);

        var active = await client.GetFromJsonAsync<List<ToolDto>>(
            $"/api/v1/tools?activeOnly=true&search={toolCode}");
        Assert.Empty(active!);

        var usage = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/usage", new { Count = 1 });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, usage.StatusCode);
    }

    [Fact]
    public async Task Return_Missing_WritesToolOff()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var toolCode = UniqueCode("TOOL");
        var workCenterId = await CreateWorkCenterAsync(client);
        await client.PostAsJsonAsync("/api/v1/tools", MinimalTool(toolCode));

        await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/checkout",
            new { WorkCenterId = workCenterId, CheckedOutBy = "operator-1" });
        var ret = await client.PostAsJsonAsync($"/api/v1/tools/{toolCode}/return",
            new { Condition = "Missing", ReturnedBy = "supervisor-1", Notes = "Không tìm thấy sau ca" });
        Assert.Equal(HttpStatusCode.NoContent, ret.StatusCode);

        var detail = await client.GetFromJsonAsync<ToolDetailDto>($"/api/v1/tools/{toolCode}");
        Assert.Equal("Scrapped", detail!.Status);
        Assert.False(detail.IsActive);
        var record = Assert.Single(detail.CheckoutHistory);
        Assert.Equal("Missing", record.ConditionOnReturn);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private static object MinimalTool(string code) => new
    {
        Code = code,
        Name = $"Tool {code}",
        ToolType = "Jig",
    };

    private static async Task<string> CreateOperationAsync(HttpClient client)
    {
        var code = UniqueCode("OP")[..10];
        var response = await client.PostAsJsonAsync("/api/v1/operations",
            new { Code = code, Name = $"Operation {code}" });
        Assert.True(response.IsSuccessStatusCode,
            $"Operation create failed: {await response.Content.ReadAsStringAsync()}");
        return code;
    }

    private static async Task<int> CreateWorkCenterAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/work-centers",
            new { Code = UniqueCode("WC"), Name = "WC for tools" });
        Assert.True(response.IsSuccessStatusCode,
            $"WorkCenter create failed: {await response.Content.ReadAsStringAsync()}");
        var body = await response.Content.ReadFromJsonAsync<WorkCenterCreated>();
        return body!.WorkCenterId;
    }

    private record WorkCenterCreated(int WorkCenterId);
}
