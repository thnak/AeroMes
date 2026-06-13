namespace AeroMes.Domain.Integration.Repositories;

public interface IProductionOrderRepository
{
    Task<ProductionOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductionOrder?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<ProductionOrder?> GetByCodeAsync(string poCode, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> HasDownstreamDocumentsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ProductionOrder>> GetBySoIdAsync(int soId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductionOrder>> GetFilteredAsync(
        int? soId, string? poCode, string? productCode,
        ProductionOrderStatus? status, CancellationToken ct = default);
    Task AddAsync(ProductionOrder entity, CancellationToken ct = default);
    void Remove(ProductionOrder entity);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<string> NextPoCodeAsync(CancellationToken ct = default);
}
