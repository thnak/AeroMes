using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendarById;
using AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendars;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class WorkCalendarTests(AeroMesWebFactory factory)
{
    // Matches the API's JsonStringEnumConverter(CamelCase) setting
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/work-calendars");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidCalendar_Returns201WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var shiftId = await CreateShiftAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/work-calendars",
            CalendarPayload(UniqueCode("CAL"), "Morning Calendar", shiftId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<WorkCalendarCreatedResult>();
        Assert.True(body?.WorkCalendarId > 0);
    }

    [Fact]
    public async Task Create_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var shiftId = await CreateShiftAsync(client);
        var code = UniqueCode("DUP");

        await client.PostAsJsonAsync("/api/v1/work-calendars", CalendarPayload(code, "First", shiftId));

        var response = await client.PostAsJsonAsync("/api/v1/work-calendars", CalendarPayload(code, "Dup", shiftId));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyDays_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/work-calendars", new
        {
            Code = UniqueCode("EMP"),
            Name = "No Days",
            Description = (string?)null,
            Days = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AfterCreate_ContainsCalendar()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var shiftId = await CreateShiftAsync(client);
        var code = UniqueCode("LST");

        await client.PostAsJsonAsync("/api/v1/work-calendars",
            CalendarPayload(code, "Listed Calendar", shiftId));

        var items = await (await client.GetAsync("/api/v1/work-calendars?activeOnly=false"))
            .Content.ReadFromJsonAsync<List<WorkCalendarDto>>();

        Assert.NotNull(items);
        Assert.Contains(items, x => x.CalendarCode == code && x.CalendarName == "Listed Calendar");
    }

    [Fact]
    public async Task GetById_ExistingCalendar_ReturnsDaysAndShifts()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var shiftId = await CreateShiftAsync(client);

        var createResp = await client.PostAsJsonAsync("/api/v1/work-calendars",
            CalendarPayload(UniqueCode("DET"), "Detail Calendar", shiftId));
        var created = await createResp.Content.ReadFromJsonAsync<WorkCalendarCreatedResult>();

        var detail = await (await client.GetAsync($"/api/v1/work-calendars/{created!.WorkCalendarId}"))
            .Content.ReadFromJsonAsync<WorkCalendarDetailDto>(JsonOpts);

        Assert.NotNull(detail);
        Assert.True(detail.IsActive);
        Assert.Single(detail.Days);
        Assert.Equal(DayOfWeek.Monday, detail.Days[0].DayOfWeek);
        Assert.True(detail.Days[0].IsWorkingDay);
        Assert.Single(detail.Days[0].Shifts);
        Assert.Equal(shiftId, detail.Days[0].Shifts[0].WorkShiftId);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/v1/work-calendars/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingCalendar_Returns204AndPersists()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var shiftId = await CreateShiftAsync(client);

        var createResp = await client.PostAsJsonAsync("/api/v1/work-calendars",
            CalendarPayload(UniqueCode("UPD"), "Original", shiftId));
        var created = await createResp.Content.ReadFromJsonAsync<WorkCalendarCreatedResult>();

        var updateResp = await client.PutAsJsonAsync($"/api/v1/work-calendars/{created!.WorkCalendarId}", new
        {
            Name = "Updated Calendar",
            Description = "new desc",
            IsActive = true,
            Days = new[]
            {
                new { DayOfWeek = DayOfWeek.Tuesday, IsWorkingDay = true,
                      Shifts = new[] { new { WorkShiftId = shiftId, Sequence = 1 } } }
            }
        });

        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        var detail = await (await client.GetAsync($"/api/v1/work-calendars/{created.WorkCalendarId}"))
            .Content.ReadFromJsonAsync<WorkCalendarDetailDto>(JsonOpts);
        Assert.Equal("Updated Calendar", detail?.CalendarName);
        Assert.Single(detail!.Days);
        Assert.Equal(DayOfWeek.Tuesday, detail.Days[0].DayOfWeek);
    }

    [Fact]
    public async Task Update_NotFound_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var shiftId = await CreateShiftAsync(client);

        var response = await client.PutAsJsonAsync("/api/v1/work-calendars/999999", new
        {
            Name = "Ghost",
            Description = (string?)null,
            IsActive = true,
            Days = new[]
            {
                new { DayOfWeek = DayOfWeek.Monday, IsWorkingDay = true,
                      Shifts = new[] { new { WorkShiftId = shiftId, Sequence = 1 } } }
            }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingCalendar_Returns204AndHidesFromActiveList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var shiftId = await CreateShiftAsync(client);

        var createResp = await client.PostAsJsonAsync("/api/v1/work-calendars",
            CalendarPayload(UniqueCode("DEL"), "To Delete", shiftId));
        var created = await createResp.Content.ReadFromJsonAsync<WorkCalendarCreatedResult>();

        var deleteResp = await client.DeleteAsync($"/api/v1/work-calendars/{created!.WorkCalendarId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        var items = await (await client.GetAsync("/api/v1/work-calendars"))
            .Content.ReadFromJsonAsync<List<WorkCalendarDto>>();
        Assert.DoesNotContain(items!, x => x.WorkCalendarId == created.WorkCalendarId);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.DeleteAsync("/api/v1/work-calendars/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddException_Holiday_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var shiftId = await CreateShiftAsync(client);

        var createResp = await client.PostAsJsonAsync("/api/v1/work-calendars",
            CalendarPayload(UniqueCode("EXC"), "Exception Cal", shiftId));
        var created = await createResp.Content.ReadFromJsonAsync<WorkCalendarCreatedResult>();

        var exResp = await client.PostAsJsonAsync(
            $"/api/v1/work-calendars/{created!.WorkCalendarId}/exceptions",
            new { Date = "2026-01-01", ExceptionType = "Holiday", WorkShiftId = (int?)null });

        Assert.Equal(HttpStatusCode.Created, exResp.StatusCode);
        var body = await exResp.Content.ReadFromJsonAsync<CalendarExceptionCreatedResult>();
        Assert.True(body?.CalendarExceptionId > 0);
    }

    [Fact]
    public async Task AddException_ExtraDay_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var shiftId = await CreateShiftAsync(client);

        var createResp = await client.PostAsJsonAsync("/api/v1/work-calendars",
            CalendarPayload(UniqueCode("EXD"), "Extra Day Cal", shiftId));
        var created = await createResp.Content.ReadFromJsonAsync<WorkCalendarCreatedResult>();

        var exResp = await client.PostAsJsonAsync(
            $"/api/v1/work-calendars/{created!.WorkCalendarId}/exceptions",
            new { Date = "2026-01-03", ExceptionType = "ExtraDay", WorkShiftId = shiftId });

        Assert.Equal(HttpStatusCode.Created, exResp.StatusCode);
    }

    [Fact]
    public async Task AddException_ExtraDayWithoutShift_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var shiftId = await CreateShiftAsync(client);

        var createResp = await client.PostAsJsonAsync("/api/v1/work-calendars",
            CalendarPayload(UniqueCode("EXV"), "Validation Cal", shiftId));
        var created = await createResp.Content.ReadFromJsonAsync<WorkCalendarCreatedResult>();

        var exResp = await client.PostAsJsonAsync(
            $"/api/v1/work-calendars/{created!.WorkCalendarId}/exceptions",
            new { Date = "2026-01-03", ExceptionType = "ExtraDay", WorkShiftId = (int?)null });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exResp.StatusCode);
    }

    [Fact]
    public async Task RemoveException_Returns204()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var shiftId = await CreateShiftAsync(client);

        var createResp = await client.PostAsJsonAsync("/api/v1/work-calendars",
            CalendarPayload(UniqueCode("REM"), "Remove Ex Cal", shiftId));
        var created = await createResp.Content.ReadFromJsonAsync<WorkCalendarCreatedResult>();

        var addExResp = await client.PostAsJsonAsync(
            $"/api/v1/work-calendars/{created!.WorkCalendarId}/exceptions",
            new { Date = "2026-02-14", ExceptionType = "Holiday", WorkShiftId = (int?)null });
        var addedEx = await addExResp.Content.ReadFromJsonAsync<CalendarExceptionCreatedResult>();

        var removeResp = await client.DeleteAsync(
            $"/api/v1/work-calendars/{created.WorkCalendarId}/exceptions/{addedEx!.CalendarExceptionId}");
        Assert.Equal(HttpStatusCode.NoContent, removeResp.StatusCode);

        var detail = await (await client.GetAsync($"/api/v1/work-calendars/{created.WorkCalendarId}"))
            .Content.ReadFromJsonAsync<WorkCalendarDetailDto>(JsonOpts);
        Assert.Empty(detail!.Exceptions);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..10].ToUpperInvariant();

    private static object CalendarPayload(string code, string name, int shiftId) => new
    {
        Code = code,
        Name = name,
        Description = (string?)null,
        Days = new[]
        {
            new
            {
                DayOfWeek = DayOfWeek.Monday,
                IsWorkingDay = true,
                Shifts = new[] { new { WorkShiftId = shiftId, Sequence = 1 } }
            }
        }
    };

    private static async Task<int> CreateShiftAsync(HttpClient client)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/work-shifts", new
        {
            Code = $"S-{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
            Name = "Test Shift",
            StartTime = "08:00:00",
            EndTime = "16:00:00",
            Breaks = Array.Empty<object>()
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<ShiftCreated>();
        return body!.WorkShiftId;
    }

    private record WorkCalendarCreatedResult(int WorkCalendarId);
    private record CalendarExceptionCreatedResult(int CalendarExceptionId);
    private record ShiftCreated(int WorkShiftId);
}
