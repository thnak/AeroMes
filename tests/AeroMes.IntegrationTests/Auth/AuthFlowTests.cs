using System.Net;
using System.Net.Http.Json;
using AeroMes.Api.Constants;
using AeroMes.Api.Controllers;

namespace AeroMes.IntegrationTests.Auth;

[Collection("Integration")]
public class AuthFlowTests(AeroMesWebFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessTokenAndRefreshCookie()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest( "system@aeromes.local", "ChangeMe123!" ), ApiJsonContext.Default.LoginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync(ApiJsonContext.Default.LoginResponse);
        Assert.NotNull(body?.AccessToken);
        Assert.Equal("Bearer", body.TokenType);
        Assert.True(body.ExpiresIn > 0);

        // Refresh cookie must be set
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies, c => c.StartsWith("refresh_token="));
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "system@aeromes.local", Password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ForcePasswordChange_FlagIsTrue_ForSeedAdmin()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "system@aeromes.local", Password = "ChangeMe123!" });

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.True(body?.ForcePasswordChange);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsUserInfo()
    {
        var token = await GetAccessTokenAsync();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Admin has ForcePasswordChange=true so most endpoints return 403.
        // /me is in the allowed list (ForcePasswordChangeMiddleware bypass).
        var response = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ForcePasswordChange_BlocksProtectedEndpoints()
    {
        var token = await GetAccessTokenAsync();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "system@aeromes.local", Password = "ChangeMe123!" });
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.AccessToken;
    }

    private record LoginResponse(
        string AccessToken, string TokenType, int ExpiresIn,
        string Email, string FullName, string[] Roles, bool ForcePasswordChange);
}
