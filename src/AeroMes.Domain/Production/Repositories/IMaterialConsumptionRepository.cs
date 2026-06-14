namespace AeroMes.Domain.Production.Repositories;

public record MaterialConsumptionDto(
    long ConsumptionId, long JobId, string ProductCode,
    string? LotNumber, decimal PlannedQty, decimal ActualQty,
    DateTime? IssuedAt, string? IssuedBy, int? LocationId);

public interface IMaterialConsumptionRepository
{
    Task AddRangeAsync(IEnumerable<MaterialConsumption> items, CancellationToken ct);
    Task<MaterialConsumption?> GetByIdAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<MaterialConsumptionDto>> GetByWorkOrderAsync(int workOrderId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
