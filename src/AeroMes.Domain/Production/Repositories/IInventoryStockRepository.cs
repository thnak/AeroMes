using AeroMes.Domain.Master;

namespace AeroMes.Domain.Production.Repositories;

public interface IInventoryStockRepository
{
    Task<IReadOnlyList<InventoryStock>> GetFilteredAsync(
        LocationType? locationType, string? productCode, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryStock>> GetByLotNumberAsync(
        string lotNumber, CancellationToken ct = default);
}
