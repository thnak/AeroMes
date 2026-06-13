namespace AeroMes.Domain.Wms.Repositories;

public interface IGoodsReceiptNoteRepository
{
    Task<IReadOnlyList<GoodsReceiptNote>> GetAllAsync(GrnStatus? status, int? poId, CancellationToken ct = default);
    Task<GoodsReceiptNote?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<GoodsReceiptNote?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(GoodsReceiptNote entity, CancellationToken ct = default);
}
