using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class SignalMappingRepository(AppDbContext db) : ISignalMappingRepository
{
    public Task<SignalMapping?> GetByIdAsync(int id, CancellationToken ct) =>
        db.SignalMappings.FirstOrDefaultAsync(x => x.SignalID == id, ct);

    public async Task<IReadOnlyList<SignalMapping>> GetByAdapterAsync(int adapterId, CancellationToken ct) =>
        await db.SignalMappings
            .Where(x => x.AdapterID == adapterId)
            .OrderBy(x => x.TagKey)
            .ToListAsync(ct);

    public Task<bool> TagKeyExistsAsync(int adapterId, string tagKey, CancellationToken ct) =>
        db.SignalMappings.AnyAsync(x => x.AdapterID == adapterId && x.TagKey == tagKey, ct);

    public Task AddAsync(SignalMapping entity, CancellationToken ct)
    {
        db.SignalMappings.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(SignalMapping entity) => db.SignalMappings.Remove(entity);
}
