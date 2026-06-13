namespace AeroMes.Domain.Integration.Repositories;

public record OrderProgressDto(
    int POID, string POCode, string ProductCode,
    int TargetQty, int ProducedOK, int ProducedNG,
    double CompletionPct, bool IsDelayed,
    DateTime? PlannedEnd, DateTime? ActualStart, DateTime? ActualEnd,
    string Status);

public record SoProductionStatusDto(
    int SOID, string SOCode, string? CustomerName, DateTime OrderDate,
    DateTime? DeliveryDate, string SoStatus,
    int TotalOrders, int CompletedOrders, int TotalTargetQty, int TotalProducedQty);

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
    Task<IReadOnlyList<OrderProgressDto>> GetProgressReportAsync(
        DateTime? from, DateTime? to, string? status, CancellationToken ct = default);
    Task<IReadOnlyList<SoProductionStatusDto>> GetSoProductionStatusAsync(
        DateTime? from, DateTime? to, CancellationToken ct = default);
}
