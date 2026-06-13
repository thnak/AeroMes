namespace AeroMes.Domain.Cost.Repositories;

public record ItemCostHistoryDto(
    int CostID, string ProductCode, string CostType,
    decimal UnitCost, string CostUoM,
    DateOnly EffectiveFrom, DateOnly? EffectiveTo,
    string? SourceRef, string? ApprovedBy, DateTime CreatedAt);

public interface IItemCostHistoryRepository
{
    Task<int> AddAsync(ItemCostHistory cost, CancellationToken ct);
    Task<ItemCostHistory?> GetActiveAsync(string productCode, ItemCostType costType, CancellationToken ct);
    Task<IReadOnlyList<ItemCostHistoryDto>> GetByProductAsync(string productCode, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
