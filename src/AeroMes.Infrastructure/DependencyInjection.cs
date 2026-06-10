using AeroMes.Application.Interfaces;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using AeroMes.Infrastructure.Identity;
using AeroMes.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroMes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(3)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // master repositories
        services.AddScoped<IWorkCenterRepository, WorkCenterRepository>();
        services.AddScoped<IMachineRepository, MachineRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IBomItemRepository, BomItemRepository>();
        services.AddScoped<IStorageLocationRepository, StorageLocationRepository>();
        services.AddScoped<IRoutingRepository, RoutingRepository>();

        // integration repositories
        services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();

        // prod repositories
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IProductionLogRepository, ProductionLogRepository>();
        services.AddScoped<IDowntimeLogRepository, DowntimeLogRepository>();

        // qual repositories
        services.AddScoped<IDefectCodeRepository, DefectCodeRepository>();

        services.AddMemoryCache();
        services.AddSingleton<IdempotencyStore>();

        services.AddIdentityCore<ApplicationUser>(opts =>
        {
            opts.Password.RequireDigit = true;
            opts.Password.RequiredLength = 8;
            opts.Password.RequireUppercase = false;
            opts.Password.RequireNonAlphanumeric = false;
            opts.Lockout.MaxFailedAccessAttempts = 5;
            opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            opts.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        // Passkey (WebAuthn) — .NET 10 built-in, no AddPasskeys() extension needed
        services.Configure<IdentityPasskeyOptions>(opts =>
        {
            opts.ServerDomain = configuration["Auth:PasskeyServerDomain"] ?? "localhost";
            var allowedOrigins = configuration.GetSection("Auth:PasskeyAllowedOrigins").Get<string[]>() ?? [];
            opts.ValidateOrigin = ctx => ValueTask.FromResult(
                allowedOrigins.Contains(ctx.Origin, StringComparer.OrdinalIgnoreCase));
        });
        services.AddScoped<IPasskeyHandler<ApplicationUser>, PasskeyHandler<ApplicationUser>>();

        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionOverrideRepository, PermissionOverrideRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

        services.AddSingleton<DbAuditLogger>();
        services.AddSingleton<IAuditLogger>(sp => sp.GetRequiredService<DbAuditLogger>());
        services.AddHostedService(sp => sp.GetRequiredService<DbAuditLogger>());

        return services;
    }
}
