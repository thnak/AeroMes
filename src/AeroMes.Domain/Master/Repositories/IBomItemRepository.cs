namespace AeroMes.Domain.Master.Repositories;

public interface IBomItemRepository
{
    Task<BomItem?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<BomItem>> GetByParentAsync(string parentCode, CancellationToken ct = default);
    Task<bool> PairExistsAsync(string parentCode, string childCode, CancellationToken ct = default);
    Task AddAsync(BomItem entity, CancellationToken ct = default);
}
