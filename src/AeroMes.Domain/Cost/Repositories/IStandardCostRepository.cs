namespace AeroMes.Domain.Cost.Repositories;

public record StandardCostSummaryDto(
    int StdCostId,
    string ProductCode,
    int CostVersion,
    string Status,
    decimal TotalMaterialCost,
    decimal TotalLaborCost,
    decimal TotalMachineCost,
    decimal TotalOverheadCost,
    decimal TotalStandardCost,
    string Currency,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string? CreatedBy,
    DateTime CreatedAt);

public interface IStandardCostRepository
{
    Task<StandardCostHeader?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<StandardCostHeader?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<StandardCostHeader?> GetActiveByProductAsync(string productCode, CancellationToken ct = default);
    Task<IReadOnlyList<StandardCostSummaryDto>> GetListAsync(string? productCode, string? status, CancellationToken ct = default);
    Task<int> NextVersionForProductAsync(string productCode, CancellationToken ct = default);
    Task AddAsync(StandardCostHeader entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
