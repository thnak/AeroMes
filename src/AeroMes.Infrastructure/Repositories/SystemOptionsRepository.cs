using AeroMes.Application.Interfaces;
using AeroMes.Domain.Settings;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class SystemOptionsRepository(AppDbContext db) : ISystemOptionsRepository
{
    public async Task<SystemOptions> GetAsync(CancellationToken ct)
    {
        var opts = await db.SystemOptions.FirstOrDefaultAsync(ct);
        if (opts is null)
        {
            opts = SystemOptions.CreateDefaults();
            db.SystemOptions.Add(opts);
            await db.SaveChangesAsync(ct);
        }
        return opts;
    }
}
