using AeroMes.Infrastructure.Data;
using AeroMes.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Testcontainers.MsSql;

namespace AeroMes.IntegrationTests;

public sealed class AeroMesWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    // Stable test user for master-data tests — no ForcePasswordChange, SystemAdmin role.
    public const string TestUserEmail = "test-admin@aeromes.local";
    public const string TestUserPassword = "TestAdmin123!";

    private bool _testUserSeeded;
    private readonly SemaphoreSlim _seedLock = new(1, 1);

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
        => await _container.StartAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace DbContext with test container
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseSqlServer(_container.GetConnectionString(),
                    sql =>
                    {
                        sql.EnableRetryOnFailure(3);
                    }));
        });

        builder.UseSetting("Jwt:Key", "test-jwt-key-must-be-at-least-32-characters-long!");
        builder.UseSetting("Jwt:Issuer", "AeroMesTest");
        builder.UseSetting("Jwt:Audience", "AeroMesTestClients");
        builder.UseSetting("Jwt:ExpiryMinutes", "15");
        builder.UseSetting("Jwt:RefreshTokenDays", "7");
        builder.UseSetting("Seeding:AdminEmail", "system@aeromes.local");
        builder.UseSetting("Seeding:AdminPassword", "ChangeMe123!");
        // Disable MFA enforcement so tests can access any endpoint without an MFA-verified token.
        builder.UseSetting("Auth:MfaRequiredRoles:0", "");
    }

    public AppDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    /// <summary>
    /// Returns an authenticated <see cref="HttpClient"/> logged in as the stable test admin.
    /// The test user is created once (idempotent) and has no ForcePasswordChange flag.
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        await EnsureTestUserAsync();

        var client = CreateClient();
        var resp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = TestUserEmail, Password = TestUserPassword });
        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadFromJsonAsync<TokenBody>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return client;
    }

    private async Task EnsureTestUserAsync()
    {
        await _seedLock.WaitAsync();
        try
        {
            if (_testUserSeeded) return;

            // Trigger host initialization so seeding runs and roles exist.
            _ = CreateClient();

            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.FindByEmailAsync(TestUserEmail) is null)
            {
                var user = new ApplicationUser
                {
                    UserName = TestUserEmail,
                    Email = TestUserEmail,
                    FullName = "Test Admin",
                    EmailConfirmed = true,
                    ForcePasswordChange = false,
                    IsActive = true,
                };
                var result = await userManager.CreateAsync(user, TestUserPassword);
                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        $"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                await userManager.AddToRoleAsync(user, AppRoles.SystemAdmin);
            }

            _testUserSeeded = true;
        }
        finally
        {
            _seedLock.Release();
        }
    }

    private record TokenBody(string AccessToken, string TokenType, int ExpiresIn,
        string Email, string FullName, string[] Roles, bool ForcePasswordChange);
}
