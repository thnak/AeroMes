using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Master.ProductionTeams.Queries.GetProductionTeamByCode;
using AeroMes.Application.Master.ProductionTeams.Queries.GetProductionTeams;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class ProductionTeamTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task Create_WithRosterAndGroups_RoundTrip()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var categoryId = await CreateCategoryAsync(client);
        var employeeCode = await CreateEmployeeAsync(client);
        var teamCode = UniqueCode("TEAM");

        var create = await client.PostAsJsonAsync("/api/v1/production-teams", new
        {
            Code = teamCode,
            Name = "Tổ cắt 1",
            StandardLaborQuantity = 8,
            ProductionRate = 12.5m,
            IsOrderBasedPlanningEnabled = true,
            ProductGroupCategoryIds = new[] { categoryId },
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var addMember = await client.PostAsJsonAsync($"/api/v1/production-teams/{teamCode}/members",
            new { EmployeeCode = employeeCode, IsLeader = true });
        Assert.Equal(HttpStatusCode.Created, addMember.StatusCode);

        var detail = await client.GetFromJsonAsync<ProductionTeamDetailDto>($"/api/v1/production-teams/{teamCode}");
        Assert.Equal("Tổ cắt 1", detail!.TeamName);
        Assert.Equal(8, detail.StandardLaborQuantity);
        Assert.Equal(12.5m, detail.ProductionRate);
        Assert.True(detail.IsOrderBasedPlanningEnabled);
        var member = Assert.Single(detail.Members);
        Assert.Equal(employeeCode, member.EmployeeCode);
        Assert.True(member.IsLeader);
        var group = Assert.Single(detail.ProductGroups);
        Assert.Equal(categoryId, group.CategoryId);
    }

    [Fact]
    public async Task Create_DuplicateCode_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var teamCode = UniqueCode("TEAM");

        var first = await client.PostAsJsonAsync("/api/v1/production-teams", MinimalTeam(teamCode));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/v1/production-teams", MinimalTeam(teamCode));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, second.StatusCode);
    }

    [Fact]
    public async Task Create_PlanningEnabledWithoutProductGroups_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/production-teams", new
        {
            Code = UniqueCode("TEAM"),
            Name = "No groups",
            IsOrderBasedPlanningEnabled = true,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task AddMember_UnknownEmployee_Returns422_AndDuplicate_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var teamCode = UniqueCode("TEAM");
        var employeeCode = await CreateEmployeeAsync(client);
        await client.PostAsJsonAsync("/api/v1/production-teams", MinimalTeam(teamCode));

        var unknown = await client.PostAsJsonAsync($"/api/v1/production-teams/{teamCode}/members",
            new { EmployeeCode = "NOPE-404" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unknown.StatusCode);

        var first = await client.PostAsJsonAsync($"/api/v1/production-teams/{teamCode}/members",
            new { EmployeeCode = employeeCode });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await client.PostAsJsonAsync($"/api/v1/production-teams/{teamCode}/members",
            new { EmployeeCode = employeeCode });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicate.StatusCode);

        var remove = await client.DeleteAsync($"/api/v1/production-teams/{teamCode}/members/{employeeCode}");
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);
    }

    [Fact]
    public async Task Duplicate_CopiesConfigurationExceptCode()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var categoryId = await CreateCategoryAsync(client);
        var employeeCode = await CreateEmployeeAsync(client);
        var sourceCode = UniqueCode("TEAM");
        var copyCode = UniqueCode("TEAM");

        await client.PostAsJsonAsync("/api/v1/production-teams", new
        {
            Code = sourceCode,
            Name = "Source team",
            StandardLaborQuantity = 5,
            ProductionRate = 7.25m,
            IsOrderBasedPlanningEnabled = true,
            ProductGroupCategoryIds = new[] { categoryId },
        });
        await client.PostAsJsonAsync($"/api/v1/production-teams/{sourceCode}/members",
            new { EmployeeCode = employeeCode, IsLeader = true });

        var duplicate = await client.PostAsJsonAsync($"/api/v1/production-teams/{sourceCode}/duplicate",
            new { NewCode = copyCode });
        Assert.Equal(HttpStatusCode.Created, duplicate.StatusCode);

        var copy = await client.GetFromJsonAsync<ProductionTeamDetailDto>($"/api/v1/production-teams/{copyCode}");
        Assert.Equal("Source team", copy!.TeamName);
        Assert.Equal(5, copy.StandardLaborQuantity);
        Assert.Equal(7.25m, copy.ProductionRate);
        Assert.True(copy.IsOrderBasedPlanningEnabled);
        Assert.Equal(employeeCode, Assert.Single(copy.Members).EmployeeCode);
        Assert.Equal(categoryId, Assert.Single(copy.ProductGroups).CategoryId);

        // Reusing an existing code is rejected.
        var clash = await client.PostAsJsonAsync($"/api/v1/production-teams/{sourceCode}/duplicate",
            new { NewCode = copyCode });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, clash.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ExcludesFromActiveList_AndDeleteSoftDeletes()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var teamCode = UniqueCode("TEAM");
        await client.PostAsJsonAsync("/api/v1/production-teams", MinimalTeam(teamCode));

        var deactivate = await client.PutAsJsonAsync($"/api/v1/production-teams/{teamCode}", new
        {
            Name = "Deactivated team",
            IsOrderBasedPlanningEnabled = false,
            IsActive = false,
        });
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);

        var active = await client.GetFromJsonAsync<List<ProductionTeamDto>>(
            $"/api/v1/production-teams?activeOnly=true&search={teamCode}");
        Assert.Empty(active!);

        var all = await client.GetFromJsonAsync<List<ProductionTeamDto>>(
            $"/api/v1/production-teams?activeOnly=false&search={teamCode}");
        Assert.Single(all!);

        var delete = await client.DeleteAsync($"/api/v1/production-teams/{teamCode}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var detail = await client.GetAsync($"/api/v1/production-teams/{teamCode}");
        Assert.Equal(HttpStatusCode.NotFound, detail.StatusCode);
    }

    [Fact]
    public async Task Update_EnablingPlanning_RequiresProductGroups()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var teamCode = UniqueCode("TEAM");
        await client.PostAsJsonAsync("/api/v1/production-teams", MinimalTeam(teamCode));

        var response = await client.PutAsJsonAsync($"/api/v1/production-teams/{teamCode}", new
        {
            Name = "Still no groups",
            IsOrderBasedPlanningEnabled = true,
            IsActive = true,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private static object MinimalTeam(string code) => new
    {
        Code = code,
        Name = $"Team {code}",
        IsOrderBasedPlanningEnabled = false,
    };

    private static async Task<int> CreateCategoryAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/product-categories",
            new { Code = UniqueCode("GRP"), Name = "Nhóm test" });
        Assert.True(response.IsSuccessStatusCode,
            $"Category create failed: {await response.Content.ReadAsStringAsync()}");
        var body = await response.Content.ReadFromJsonAsync<CategoryCreated>();
        return body!.CategoryId;
    }

    private static async Task<string> CreateEmployeeAsync(HttpClient client)
    {
        var code = UniqueCode("EMP");
        var response = await client.PostAsJsonAsync("/api/v1/employees",
            new { Code = code, FullName = $"Worker {code}", RoleType = "Operator" });
        Assert.True(response.IsSuccessStatusCode,
            $"Employee create failed: {await response.Content.ReadAsStringAsync()}");
        return code;
    }

    private record CategoryCreated(int CategoryId);
}
