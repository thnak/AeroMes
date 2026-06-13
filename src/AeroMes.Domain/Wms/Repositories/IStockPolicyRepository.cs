namespace AeroMes.Domain.Wms.Repositories;

public interface IStockPolicyRepository
{
    Task<IReadOnlyList<StockPolicy>> GetAllAsync(bool? isActive = null, CancellationToken ct = default);
    Task<StockPolicy?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<StockPolicy?> GetActiveByProductAndLocationAsync(string productCode, int locationId, CancellationToken ct = default);
    Task<bool> ExistsForProductAndLocationAsync(string productCode, int locationId, int? excludeId, CancellationToken ct = default);
    Task AddAsync(StockPolicy policy, CancellationToken ct = default);
}
