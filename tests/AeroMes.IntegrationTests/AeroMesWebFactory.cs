using AeroMes.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AeroMes.IntegrationTests;

public sealed class AeroMesWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

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
                    sql => sql.EnableRetryOnFailure(3)));
        });

        builder.UseSetting("Jwt:Key", "test-jwt-key-must-be-at-least-32-characters-long!");
        builder.UseSetting("Jwt:Issuer", "AeroMesTest");
        builder.UseSetting("Jwt:Audience", "AeroMesTestClients");
        builder.UseSetting("Jwt:ExpiryMinutes", "15");
        builder.UseSetting("Jwt:RefreshTokenDays", "7");
        builder.UseSetting("Seeding:AdminEmail", "system@aeromes.local");
        builder.UseSetting("Seeding:AdminPassword", "ChangeMe123!");
    }

    public AppDbContext CreateDbContext()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;
        // AppDbContext requires IPublisher; use the factory's service provider instead
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
