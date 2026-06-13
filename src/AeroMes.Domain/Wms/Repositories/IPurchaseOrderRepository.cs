namespace AeroMes.Domain.Wms.Repositories;

public interface IPurchaseOrderRepository
{
    Task<IReadOnlyList<PurchaseOrder>> GetAllAsync(PoStatus? status, string? supplierCode, CancellationToken ct = default);
    Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PurchaseOrder?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(PurchaseOrder entity, CancellationToken ct = default);
}
