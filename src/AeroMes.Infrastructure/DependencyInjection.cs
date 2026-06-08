using AeroMes.Application.Interfaces;
using AeroMes.Domain.Equipment.Repositories;
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

        // IUnitOfWork is fulfilled by AppDbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Repositories
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IProductionLogRepository, ProductionLogRepository>();
        services.AddScoped<IDefectCodeRepository, DefectCodeRepository>();
        services.AddScoped<IWorkCenterRepository, WorkCenterRepository>();
        services.AddScoped<IDowntimeLogRepository, DowntimeLogRepository>();

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

        return services;
    }
}
