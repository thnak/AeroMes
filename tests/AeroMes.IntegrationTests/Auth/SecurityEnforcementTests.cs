using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace AeroMes.IntegrationTests.Auth;

/// <summary>
/// Integration tests for security enforcement — closes #260.
/// </summary>
[Collection("Integration")]
public class SecurityEnforcementTests(AeroMesWebFactory factory)
{
    // ── JWT Bearer auth ────────────────────────────────────────────────────

    [Fact]
    public async Task BearerToken_AccessesProtectedEndpoint_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/work-orders");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── Password & account lifecycle ───────────────────────────────────────

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = AeroMesWebFactory.TestUserEmail,
            Password = "WrongPassword999!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Login_NonExistentUser_Returns401()
    {
        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = "nobody@nowhere.test",
            Password = "AnyPassword123!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Token validation ───────────────────────────────────────────────────

    [Fact]
    public async Task InvalidBearerToken_Returns401()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "this.is.not.a.valid.token");
        var resp = await client.GetAsync("/api/v1/work-orders");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task MalformedToken_Returns401()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "definitelynotjwt");
        var resp = await client.GetAsync("/api/v1/jobs");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Public endpoints (no auth required) ───────────────────────────────

    [Fact]
    public async Task AppInfo_NoAuth_Returns200()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/app-info");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Health_NoAuth_Returns200()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/health");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    // ── API Key management ─────────────────────────────────────────────────

    [Fact]
    public async Task GetApiKeys_Unauthenticated_Returns401()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/api-keys");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task CreateApiKey_ValidRequest_Returns201()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.PostAsJsonAsync("/api/v1/api-keys", new
        {
            keyName = $"TestKey-{Guid.NewGuid():N}"[..20],
            assignedRole = (string?)null,
            workCenterId = (int?)null,
            expiresAt = (DateTimeOffset?)DateTimeOffset.UtcNow.AddYears(1),
            notes = "Integration test key"
        });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var result = await resp.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.PlainKey);
    }

    [Fact]
    public async Task GetApiKeys_AfterCreate_Returns200WithList()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/v1/api-keys", new
        {
            keyName = $"ListTestKey-{Guid.NewGuid():N}"[..20],
            assignedRole = (string?)null,
            workCenterId = (int?)null,
            expiresAt = (DateTimeOffset?)DateTimeOffset.UtcNow.AddYears(1),
            notes = (string?)null
        });
        var resp = await client.GetAsync("/api/v1/api-keys");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task RevokeApiKey_AfterCreate_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var createResp = await client.PostAsJsonAsync("/api/v1/api-keys", new
        {
            keyName = $"RevokeKey-{Guid.NewGuid():N}"[..20],
            assignedRole = (string?)null,
            workCenterId = (int?)null,
            expiresAt = (DateTimeOffset?)DateTimeOffset.UtcNow.AddYears(1),
            notes = (string?)null
        });
        var key = await createResp.Content.ReadFromJsonAsync<CreateApiKeyResponse>();

        var revokeResp = await client.DeleteAsync($"/api/v1/api-keys/{key!.KeyId}");
        Assert.True(revokeResp.IsSuccessStatusCode,
            $"Revoke failed: {await revokeResp.Content.ReadAsStringAsync()}");
    }

    // ── Refresh token security ─────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_InvalidToken_Returns401()
    {
        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            RefreshToken = "fake-refresh-token-that-doesnt-exist"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Role-based access ──────────────────────────────────────────────────

    [Fact]
    public async Task ProtectedEndpoint_ValidAdmin_Returns200()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var resp = await client.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}

file record CreateApiKeyResponse(int ApiKeyId, string FullKey, string KeyPrefix, string KeyName)
{
    public string PlainKey => FullKey;
    public int KeyId => ApiKeyId;
}
