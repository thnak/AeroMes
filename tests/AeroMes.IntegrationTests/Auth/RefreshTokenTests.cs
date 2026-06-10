using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AeroMes.IntegrationTests.Auth;

[Collection("Integration")]
public class RefreshTokenTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task Refresh_WithValidCookie_ReturnsNewAccessToken()
    {
        var handler = new HttpClientHandler { UseCookies = true };
        using var client = factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            { HandleCookies = true });

        await client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "system@aeromes.local", Password = "ChangeMe123!" });

        var refreshResponse = await client.PostAsync("/api/v1/auth/refresh", null);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var body = await refreshResponse.Content.ReadFromJsonAsync<RefreshResponse>();
        Assert.NotNull(body?.AccessToken);
    }

    [Fact]
    public async Task Refresh_WithoutCookie_Returns401()
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsync("/api/v1/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_TokenRotation_OldCookieRejectedAfterRefresh()
    {
        // Use a client that tracks cookies
        using var client = factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            { HandleCookies = true });

        // Login — get initial refresh cookie
        await client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "system@aeromes.local", Password = "ChangeMe123!" });

        // Capture the first refresh cookie before rotation
        // First refresh — rotates the token
        var firstRefresh = await client.PostAsync("/api/v1/auth/refresh", null);
        Assert.Equal(HttpStatusCode.OK, firstRefresh.StatusCode);

        // Second refresh with the same client (which holds the rotated cookie) — should also succeed
        var secondRefresh = await client.PostAsync("/api/v1/auth/refresh", null);
        Assert.Equal(HttpStatusCode.OK, secondRefresh.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReuseAttack_ReplaysRevokedToken_RevokesFamily()
    {
        // Use a client with no automatic cookie management so we control cookies manually.
        using var client = factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            { HandleCookies = false });

        // Step 1: Login — capture C1 from Set-Cookie header.
        var loginResp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "system@aeromes.local", Password = "ChangeMe123!" });
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

        var c1 = ExtractRefreshTokenValue(loginResp);
        Assert.NotNull(c1);

        // Step 2: Refresh with C1 → token rotated, C1 revoked, C2 issued.
        var firstRefresh = await PostWithRefreshCookie(client, c1);
        Assert.Equal(HttpStatusCode.OK, firstRefresh.StatusCode);

        var c2 = ExtractRefreshTokenValue(firstRefresh);
        Assert.NotNull(c2);
        Assert.NotEqual(c1, c2);

        // Step 3: Replay the now-revoked C1 → reuse attack detected, entire family revoked.
        var reuseResp = await PostWithRefreshCookie(client, c1);
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResp.StatusCode);

        // Step 4: C2 must also be rejected — family revocation wiped the rotated token too.
        var c2Resp = await PostWithRefreshCookie(client, c2);
        Assert.Equal(HttpStatusCode.Unauthorized, c2Resp.StatusCode);
    }

    private static string? ExtractRefreshTokenValue(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return null;
        var cookie = cookies.FirstOrDefault(c => c.StartsWith("refresh_token="));
        if (cookie is null) return null;
        // "refresh_token=VALUE; Path=/...; HttpOnly; ..." — take the raw value only
        return cookie["refresh_token=".Length..].Split(';')[0];
    }

    private static Task<HttpResponseMessage> PostWithRefreshCookie(HttpClient client, string tokenValue)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        req.Headers.Add("Cookie", $"refresh_token={tokenValue}");
        return client.SendAsync(req);
    }

    private record RefreshResponse(string AccessToken, string TokenType, int ExpiresIn);
}
