namespace AeroMes.Domain.Integration.Repositories;

public interface IMultiProductionOrderRepository
{
    Task<MultiProductionOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MultiProductionOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<IReadOnlyList<MultiProductionOrder>> GetFilteredAsync(
        MultiProductionOrderType? orderType,
        MultiProductionOrderStatus? status,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);
    Task AddAsync(MultiProductionOrder entity, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
