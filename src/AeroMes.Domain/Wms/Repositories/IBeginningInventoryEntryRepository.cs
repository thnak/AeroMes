namespace AeroMes.Domain.Wms.Repositories;

public interface IBeginningInventoryEntryRepository
{
    Task<IReadOnlyList<BeginningInventoryEntry>> GetAllAsync(
        int? warehouseId,
        string? productCode,
        DateOnly? period,
        CancellationToken ct = default);

    Task<BeginningInventoryEntry?> GetByIdAsync(int id, CancellationToken ct = default);

    Task AddAsync(BeginningInventoryEntry entry, CancellationToken ct = default);
}
