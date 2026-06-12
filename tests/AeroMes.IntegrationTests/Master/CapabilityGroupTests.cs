using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Master.CapabilityGroups.Queries.GetCapabilityGroups;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class CapabilityGroupTests(AeroMesWebFactory factory)
{
    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/capability-groups");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidPayload_Returns201WithCode()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("CAP");

        var response = await client.PostAsJsonAsync("/api/v1/capability-groups",
            new { Code = code, Name = "Welding Capability", Description = "All welding operations" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreatedResult>();
        Assert.Equal(code, body?.GroupCode);
    }

    [Fact]
    public async Task Create_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DUP");

        await client.PostAsJsonAsync("/api/v1/capability-groups", new { Code = code, Name = "First" });

        var response = await client.PostAsJsonAsync("/api/v1/capability-groups", new { Code = code, Name = "Duplicate" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyName_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/capability-groups",
            new { Code = UniqueCode("NNM"), Name = "" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AfterCreate_ContainsCreatedGroup()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("LST");

        await client.PostAsJsonAsync("/api/v1/capability-groups",
            new { Code = code, Name = "Listed Group", Description = "desc" });

        var response = await client.GetAsync("/api/v1/capability-groups?activeOnly=false");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<CapabilityGroupDto>>();
        Assert.NotNull(items);
        Assert.Contains(items, x => x.GroupCode == code && x.GroupName == "Listed Group");
    }

    [Fact]
    public async Task Update_ExistingGroup_Returns204AndPersistsChange()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("UPD");

        await client.PostAsJsonAsync("/api/v1/capability-groups", new { Code = code, Name = "Original" });

        var updateResp = await client.PutAsJsonAsync($"/api/v1/capability-groups/{code}",
            new { Name = "Updated", Description = "updated desc", IsActive = true });

        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        var items = await (await client.GetAsync("/api/v1/capability-groups?activeOnly=false"))
            .Content.ReadFromJsonAsync<List<CapabilityGroupDto>>();
        Assert.Contains(items!, x => x.GroupCode == code && x.GroupName == "Updated");
    }

    [Fact]
    public async Task Update_NonExistent_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        // Validator checks existence first → ValidationException → 422
        var response = await client.PutAsJsonAsync("/api/v1/capability-groups/GHOST-404",
            new { Name = "Ghost", Description = (string?)null, IsActive = true });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingGroup_Returns204AndHidesFromActiveList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DEL");

        await client.PostAsJsonAsync("/api/v1/capability-groups", new { Code = code, Name = "To Delete" });

        var deleteResp = await client.DeleteAsync($"/api/v1/capability-groups/{code}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        var items = await (await client.GetAsync("/api/v1/capability-groups"))
            .Content.ReadFromJsonAsync<List<CapabilityGroupDto>>();
        Assert.DoesNotContain(items!, x => x.GroupCode == code);
    }

    [Fact]
    public async Task Delete_NonExistent_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        // Handler throws EntityNotFoundException → 404
        var response = await client.DeleteAsync("/api/v1/capability-groups/NOTHERE-404");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private record CreatedResult(string GroupCode);
}
