using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class SignalTagRepository(AppDbContext db) : ISignalTagRepository
{
    public Task<SignalTag?> GetByKeyAsync(string key, CancellationToken ct = default) =>
        db.SignalTags.FirstOrDefaultAsync(x => x.Key == key, ct);

    public async Task<IReadOnlyList<SignalTag>> GetListAsync(string? category, string? dataType, CancellationToken ct = default)
    {
        var query = db.SignalTags.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.Category == category);

        if (!string.IsNullOrWhiteSpace(dataType))
            query = query.Where(x => x.DataType == dataType);

        return await query.OrderBy(x => x.Category).ThenBy(x => x.Key).ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default) =>
        db.SignalTags.AnyAsync(x => x.Key == key, ct);

    public Task<bool> IsInUseAsync(string key, CancellationToken ct = default) =>
        db.SignalMappings.AnyAsync(s => s.TagKey == key, ct);

    public void Add(SignalTag tag) => db.SignalTags.Add(tag);

    public void Remove(SignalTag tag) => db.SignalTags.Remove(tag);
}
