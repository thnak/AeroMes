using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Master.OrgUnits.Commands.SyncOrgUnits;
using AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnitById;
using AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnits;
using AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnitTree;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class OrgUnitTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task Sync_CreatesHierarchy_AndTreeReflectsIt()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var prefix = UniquePrefix();

        var result = await SyncAsync(client,
            Unit($"{prefix}-CO", "Công ty TNHH Demo", null, "Company"),
            Unit($"{prefix}-D1", "Phân xưởng 1", $"{prefix}-CO", "Department"),
            Unit($"{prefix}-T1", "Tổ cắt", $"{prefix}-D1", "Team"));

        // Deactivated count is not asserted — full-snapshot semantics deactivate
        // units left over from other tests sharing the database.
        Assert.Equal(3, result.Created);

        var tree = await client.GetFromJsonAsync<List<OrgUnitTreeDto>>("/api/v1/org-units/tree");
        var root = Assert.Single(tree!, x => x.UnitCode == $"{prefix}-CO");
        var dept = Assert.Single(root.Children);
        Assert.Equal($"{prefix}-D1", dept.UnitCode);
        Assert.Equal(1, dept.Level);
        var team = Assert.Single(dept.Children);
        Assert.Equal($"{prefix}-T1", team.UnitCode);
        Assert.Equal(2, team.Level);
    }

    [Fact]
    public async Task Sync_MissingUnit_IsDeactivatedNotDeleted()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var prefix = UniquePrefix();

        await SyncAsync(client,
            Unit($"{prefix}-CO", "Company", null, "Company"),
            Unit($"{prefix}-D1", "Dept 1", $"{prefix}-CO", "Department"));

        // Second snapshot no longer contains D1 → it must be deactivated.
        var second = await SyncAsync(client,
            Unit($"{prefix}-CO", "Company", null, "Company"));
        Assert.Equal(1, second.Updated);

        var all = await client.GetFromJsonAsync<List<OrgUnitDto>>(
            $"/api/v1/org-units?activeOnly=false&search={prefix}-D1");
        var d1 = Assert.Single(all!);
        Assert.False(d1.IsActive);

        var active = await client.GetFromJsonAsync<List<OrgUnitDto>>(
            $"/api/v1/org-units?activeOnly=true&search={prefix}-D1");
        Assert.Empty(active!);
    }

    [Fact]
    public async Task Sync_Resync_UpdatesNameAndReparents()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var prefix = UniquePrefix();

        await SyncAsync(client,
            Unit($"{prefix}-CO", "Company", null, "Company"),
            Unit($"{prefix}-D1", "Old name", $"{prefix}-CO", "Department"),
            Unit($"{prefix}-D2", "Dept 2", $"{prefix}-CO", "Department"));

        await SyncAsync(client,
            Unit($"{prefix}-CO", "Company", null, "Company"),
            Unit($"{prefix}-D1", "New name", $"{prefix}-D2", "Department"),
            Unit($"{prefix}-D2", "Dept 2", $"{prefix}-CO", "Department"));

        var all = await client.GetFromJsonAsync<List<OrgUnitDto>>(
            $"/api/v1/org-units?search={prefix}-D1");
        var d1 = Assert.Single(all!);
        Assert.Equal("New name", d1.UnitName);
        Assert.Equal(2, d1.Level); // CO → D2 → D1

        var detail = await client.GetFromJsonAsync<OrgUnitDetailDto>($"/api/v1/org-units/{d1.UnitId}");
        Assert.Equal([$"{prefix}-CO", $"{prefix}-D2"], detail!.ParentChain.Select(x => x.UnitCode));
    }

    [Fact]
    public async Task Sync_UnknownParentCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var prefix = UniquePrefix();

        var response = await client.PostAsJsonAsync("/api/v1/org-units/sync", new
        {
            Units = new[] { Unit($"{prefix}-D1", "Orphan", $"{prefix}-NOPE", "Department") },
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Sync_EmptySnapshot_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/v1/org-units/sync",
            new { Units = Array.Empty<object>() });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task DirectCrud_IsNotExposed()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var post = await client.PostAsJsonAsync("/api/v1/org-units", new { UnitCode = "X", UnitName = "X" });
        Assert.Equal(HttpStatusCode.MethodNotAllowed, post.StatusCode);

        var put = await client.PutAsJsonAsync("/api/v1/org-units/1", new { UnitName = "X" });
        Assert.Equal(HttpStatusCode.MethodNotAllowed, put.StatusCode);

        var delete = await client.DeleteAsync("/api/v1/org-units/1");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, delete.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniquePrefix()
        => $"OU{Guid.NewGuid():N}"[..8].ToUpperInvariant();

    private static object Unit(string code, string name, string? parentCode, string unitType) => new
    {
        UnitCode = code,
        UnitName = name,
        ParentUnitCode = parentCode,
        UnitType = unitType,
        IsActive = true,
        SourceSystemId = $"amis-{code}",
    };

    private static async Task<SyncOrgUnitsResult> SyncAsync(HttpClient client, params object[] units)
    {
        var response = await client.PostAsJsonAsync("/api/v1/org-units/sync", new { Units = units });
        Assert.True(response.IsSuccessStatusCode,
            $"Sync failed: {await response.Content.ReadAsStringAsync()}");
        return (await response.Content.ReadFromJsonAsync<SyncOrgUnitsResult>())!;
    }
}
