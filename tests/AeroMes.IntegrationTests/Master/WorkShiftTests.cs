using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Master.WorkShifts.Queries.GetWorkShiftById;
using AeroMes.Application.Master.WorkShifts.Queries.GetWorkShifts;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class WorkShiftTests(AeroMesWebFactory factory)
{
    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/work-shifts");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidShift_Returns201WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/work-shifts", new
        {
            Code = UniqueCode("MS"),
            Name = "Morning Shift",
            StartTime = "06:00:00",
            EndTime = "14:00:00",
            Breaks = new[] { new { BreakStart = "10:00:00", BreakEnd = "10:15:00" } }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreatedResult>();
        Assert.True(body?.WorkShiftId > 0);
    }

    [Fact]
    public async Task Create_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DUP");

        await client.PostAsJsonAsync("/api/v1/work-shifts", ShiftPayload(code, "First"));

        var response = await client.PostAsJsonAsync("/api/v1/work-shifts", ShiftPayload(code, "Duplicate"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_SameStartAndEndTime_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/work-shifts", new
        {
            Code = UniqueCode("BAD"),
            Name = "Bad Shift",
            StartTime = "08:00:00",
            EndTime = "08:00:00",
            Breaks = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AfterCreate_ContainsShift()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("LST");

        var createResp = await client.PostAsJsonAsync("/api/v1/work-shifts", ShiftPayload(code, "Listed Shift"));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var listResp = await client.GetAsync("/api/v1/work-shifts?activeOnly=false");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);

        var items = await listResp.Content.ReadFromJsonAsync<List<WorkShiftDto>>();
        Assert.NotNull(items);
        Assert.Contains(items, x => x.ShiftCode == code && x.ShiftName == "Listed Shift");
    }

    [Fact]
    public async Task GetById_ExistingShift_ReturnsDetailWithBreaks()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DET");

        var createResp = await client.PostAsJsonAsync("/api/v1/work-shifts", new
        {
            Code = code,
            Name = "Detail Shift",
            StartTime = "22:00:00",
            EndTime = "06:00:00",
            Breaks = new[] { new { BreakStart = "02:00:00", BreakEnd = "02:30:00" } }
        });
        var created = await createResp.Content.ReadFromJsonAsync<CreatedResult>();

        var getResp = await client.GetAsync($"/api/v1/work-shifts/{created!.WorkShiftId}");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

        var detail = await getResp.Content.ReadFromJsonAsync<WorkShiftDetailDto>();
        Assert.NotNull(detail);
        Assert.True(detail.IsNightShift);
        Assert.Single(detail.Breaks);
        Assert.Equal(30, detail.Breaks[0].BreakMinutes);
        Assert.True(detail.NetMinutes > 0);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/work-shifts/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingShift_Returns204AndPersists()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("UPD");

        var createResp = await client.PostAsJsonAsync("/api/v1/work-shifts", ShiftPayload(code, "Original"));
        var created = await createResp.Content.ReadFromJsonAsync<CreatedResult>();

        var updateResp = await client.PutAsJsonAsync($"/api/v1/work-shifts/{created!.WorkShiftId}", new
        {
            Name = "Updated Shift",
            StartTime = "07:00:00",
            EndTime = "15:00:00",
            Breaks = Array.Empty<object>(),
            IsActive = true
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        var detail = await (await client.GetAsync($"/api/v1/work-shifts/{created.WorkShiftId}"))
            .Content.ReadFromJsonAsync<WorkShiftDetailDto>();
        Assert.Equal("Updated Shift", detail?.ShiftName);
        Assert.Empty(detail!.Breaks);
    }

    [Fact]
    public async Task Update_NotFound_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        // Validator checks existence first → 422
        var response = await client.PutAsJsonAsync("/api/v1/work-shifts/999999", new
        {
            Name = "Ghost",
            StartTime = "08:00:00",
            EndTime = "16:00:00",
            Breaks = Array.Empty<object>(),
            IsActive = true
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingShift_Returns204AndHidesFromActiveList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DEL");

        var createResp = await client.PostAsJsonAsync("/api/v1/work-shifts", ShiftPayload(code, "To Delete"));
        var created = await createResp.Content.ReadFromJsonAsync<CreatedResult>();

        var deleteResp = await client.DeleteAsync($"/api/v1/work-shifts/{created!.WorkShiftId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        var items = await (await client.GetAsync("/api/v1/work-shifts"))
            .Content.ReadFromJsonAsync<List<WorkShiftDto>>();
        Assert.DoesNotContain(items!, x => x.WorkShiftId == created.WorkShiftId);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.DeleteAsync("/api/v1/work-shifts/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task NightShift_NetMinutesCalculatedCorrectly()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("NGT");

        // 22:00 → 06:00 = 8 h = 480 min, minus 30-min break → 450 min
        await client.PostAsJsonAsync("/api/v1/work-shifts", new
        {
            Code = code,
            Name = "Night Shift",
            StartTime = "22:00:00",
            EndTime = "06:00:00",
            Breaks = new[] { new { BreakStart = "02:00:00", BreakEnd = "02:30:00" } }
        });

        var items = await (await client.GetAsync("/api/v1/work-shifts?activeOnly=false"))
            .Content.ReadFromJsonAsync<List<WorkShiftDto>>();
        var shift = items!.First(x => x.ShiftCode == code);

        Assert.True(shift.IsNightShift);
        Assert.Equal(450, shift.NetMinutes);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..10].ToUpperInvariant();

    private static object ShiftPayload(string code, string name) => new
    {
        Code = code,
        Name = name,
        StartTime = "08:00:00",
        EndTime = "16:00:00",
        Breaks = new[] { new { BreakStart = "12:00:00", BreakEnd = "12:30:00" } }
    };

    private record CreatedResult(int WorkShiftId);
}
