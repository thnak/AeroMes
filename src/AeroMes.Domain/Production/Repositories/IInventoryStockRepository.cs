using AeroMes.Domain.Master;

namespace AeroMes.Domain.Production.Repositories;

public interface IInventoryStockRepository
{
    Task<IReadOnlyList<InventoryStock>> GetFilteredAsync(
        LocationType? locationType, string? productCode, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryStock>> GetByLotNumberAsync(
        string lotNumber, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryStock>> GetByBinAsync(int binId, CancellationToken ct = default);
    Task<int> CountByBinAsync(int binId, CancellationToken ct = default);
    Task<InventoryStock?> FindByKeyAsync(int locationId, string productCode, string lotNumber, CancellationToken ct = default);
    Task AddAsync(InventoryStock entity, CancellationToken ct = default);
}
