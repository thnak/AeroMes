namespace AeroMes.Domain.Wms.Repositories;

public record StockMovementDto(
    long MovementId, string MovementType, string ProductCode,
    string LotNumber, decimal Quantity, int LocationId,
    string Reference, string? Notes, string? CreatedBy, DateTime CreatedAt);

public interface IStockMovementRepository
{
    Task AddAsync(StockMovement entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<StockMovement> entities, CancellationToken ct = default);
    Task<IReadOnlyList<StockMovementDto>> GetListAsync(
        string? productCode, string? lotNumber, int page, int pageSize, CancellationToken ct = default);
}
