using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using AeroMes.Api.Constants;
using AeroMes.Api.Controllers;

namespace AeroMes.IntegrationTests.Auth;

/// <summary>
/// Regression tests for GET /api/v1/app-info.
///
/// Bug: ForcePasswordChangeMiddleware and MfaEnforcementMiddleware were blocking
/// this endpoint (403) because it was missing from their allowlists. The frontend
/// interprets MFA_REQUIRED 403s by removing the access token and redirecting to
/// login, causing an infinite post-login loop.
/// </summary>
[Collection("Integration")]
public class AppInfoTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task Get_WithoutToken_Returns401()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/app-info");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithValidToken_Returns200()
    {
        var token = await GetAccessTokenAsync();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/app-info");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithValidToken_ReturnsVersionFields()
    {
        var token = await GetAccessTokenAsync();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var body = await client.GetFromJsonAsync<AppInfoEnvelope>("/api/v1/app-info");
        Assert.NotNull(body?.Data);
        Assert.NotEmpty(body!.Data!.Version);
        Assert.NotEmpty(body.Data.Environment);
        Assert.NotEmpty(body.Data.InstanceId);
    }

    /// <summary>
    /// Regression: the seed admin has ForcePasswordChange=true.
    /// Before the fix, ForcePasswordChangeMiddleware blocked /api/v1/app-info with
    /// 403 PasswordChangeRequired, which the frontend interpreted as a redirect to
    /// /auth/change-password — even though the user had not yet reached the main app.
    /// After the fix the endpoint must return 200 for this user.
    /// </summary>
    [Fact]
    public async Task Get_ForcePasswordChange_IsNotBlocked_Returns200()
    {
        var loginResp = await factory.CreateClient().PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("system@aeromes.local", "ChangeMe123!"),
            ApiJsonContext.Default.LoginRequest);

        var login = await loginResp.Content.ReadFromJsonAsync<LoginResponseBody>();
        Assert.True(login!.ForcePasswordChange, "Precondition: seed admin must have ForcePasswordChange=true");

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var response = await client.GetAsync("/api/v1/app-info");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<string> GetAccessTokenAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("system@aeromes.local", "ChangeMe123!"),
            ApiJsonContext.Default.LoginRequest);
        var body = await response.Content.ReadFromJsonAsync<LoginResponseBody>();
        return body!.AccessToken;
    }

    private record LoginResponseBody(
        string AccessToken,
        string TokenType,
        int ExpiresIn,
        string Email,
        string FullName,
        string[] Roles,
        bool ForcePasswordChange);

    private record AppInfoEnvelope(bool Success, string Message, AppInfoBody? Data);

    private record AppInfoBody(
        string Version,
        string BuildDate,
        string Environment,
        string? CommitSha,
        string InstanceId);
}
