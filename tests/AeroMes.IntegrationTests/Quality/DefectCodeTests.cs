using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Quality.DefectCodes.Queries.GetDefectCodes;

namespace AeroMes.IntegrationTests.Quality;

[Collection("Integration")]
public class DefectCodeTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/quality/defect-codes");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidPayload_Returns201WithId()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/quality/defect-codes", new
        {
            Code = UniqueCode("SCR"),
            DefectName = "Surface Scratch",
            DefectCategory = "Visual"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreatedResult>();
        Assert.True(body?.DefectCodeId > 0);
    }

    [Fact]
    public async Task Create_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DUP");

        await client.PostAsJsonAsync("/api/v1/quality/defect-codes",
            new { Code = code, DefectName = "First", DefectCategory = (string?)null });

        var response = await client.PostAsJsonAsync("/api/v1/quality/defect-codes",
            new { Code = code, DefectName = "Duplicate", DefectCategory = (string?)null });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyName_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/quality/defect-codes",
            new { Code = UniqueCode("NNM"), DefectName = "", DefectCategory = (string?)null });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AfterCreate_ContainsCreated()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("LST");

        await client.PostAsJsonAsync("/api/v1/quality/defect-codes",
            new { Code = code, DefectName = "Listed Defect", DefectCategory = "Dimensional" });

        var items = await (await client.GetAsync("/api/v1/quality/defect-codes?activeOnly=false"))
            .Content.ReadFromJsonAsync<List<DefectCodeDto>>();

        Assert.NotNull(items);
        Assert.Contains(items, x => x.Code == code && x.DefectName == "Listed Defect");
    }

    [Fact]
    public async Task Update_ExistingCode_Returns204AndPersists()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("UPD");

        var createResp = await client.PostAsJsonAsync("/api/v1/quality/defect-codes",
            new { Code = code, DefectName = "Original", DefectCategory = (string?)null });
        var created = await createResp.Content.ReadFromJsonAsync<CreatedResult>();

        var updateResp = await client.PutAsJsonAsync($"/api/v1/quality/defect-codes/{created!.DefectCodeId}",
            new { DefectName = "Updated Name", DefectCategory = "Visual", IsActive = true });

        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        var items = await (await client.GetAsync("/api/v1/quality/defect-codes?activeOnly=false"))
            .Content.ReadFromJsonAsync<List<DefectCodeDto>>();
        var updated = items!.First(x => x.DefectCodeId == created.DefectCodeId);
        Assert.Equal("Updated Name", updated.DefectName);
        Assert.Equal("Visual", updated.DefectCategory);
    }

    [Fact]
    public async Task Update_NotFound_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync("/api/v1/quality/defect-codes/999999",
            new { DefectName = "Ghost", DefectCategory = (string?)null, IsActive = true });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingCode_Returns204AndHidesFromActiveList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var code = UniqueCode("DEL");

        var createResp = await client.PostAsJsonAsync("/api/v1/quality/defect-codes",
            new { Code = code, DefectName = "To Delete", DefectCategory = (string?)null });
        var created = await createResp.Content.ReadFromJsonAsync<CreatedResult>();

        var deleteResp = await client.DeleteAsync($"/api/v1/quality/defect-codes/{created!.DefectCodeId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        var items = await (await client.GetAsync("/api/v1/quality/defect-codes"))
            .Content.ReadFromJsonAsync<List<DefectCodeDto>>();
        Assert.DoesNotContain(items!, x => x.DefectCodeId == created.DefectCodeId);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var response = await client.DeleteAsync("/api/v1/quality/defect-codes/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..10].ToUpperInvariant();

    private record CreatedResult(int DefectCodeId);
}
