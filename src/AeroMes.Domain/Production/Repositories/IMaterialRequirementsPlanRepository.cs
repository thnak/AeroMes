using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Repositories;

public record MrpListDto(
    int MrpID, string PlanNumber, string PlanName,
    int? MasterPlanId, string? OrganizationalUnit,
    DateOnly PeriodStart, DateOnly PeriodEnd,
    string Status, int LineCount, DateTime CreatedAt);

public record MrpLineDto(
    int MrpLineID, string FinishedGoodCode, decimal FinishedGoodQty,
    string MaterialCode, string MaterialName, string UnitOfMeasure,
    decimal FixedWaste, decimal WasteRatio, decimal CalculatedMaterialQty,
    decimal OpeningInventory, decimal ConcurrentPurchaseRequestQty,
    decimal PlannedOrderQty, decimal ForecastedClosingBalance, bool HasShortfall);

public record MrpDetailDto(
    int MrpID, string PlanNumber, string PlanName,
    int? MasterPlanId, string? OrganizationalUnit,
    DateOnly PeriodStart, DateOnly PeriodEnd,
    string Status, string? Notes, DateTime CreatedAt,
    IReadOnlyList<MrpLineDto> Lines);

public interface IMaterialRequirementsPlanRepository
{
    Task<int> AddAsync(MaterialRequirementsPlan plan, CancellationToken ct);
    Task<MaterialRequirementsPlan?> GetByIdAsync(int mrpId, CancellationToken ct);
    Task<bool> PlanNumberExistsAsync(string planNumber, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task DeleteAsync(MaterialRequirementsPlan plan, CancellationToken ct);

    Task<(IReadOnlyList<MrpListDto> Items, int Total)> GetListAsync(
        string? keyword, int? masterPlanId, string? status, int page, int pageSize, CancellationToken ct);

    Task<MrpDetailDto?> GetDetailAsync(int mrpId, CancellationToken ct);

    Task<IReadOnlyList<BomExplosionItem>> ExplodeBomAsync(
        int masterPlanId, DateOnly date, CancellationToken ct);
}

public record BomExplosionItem(
    string FinishedGoodCode, decimal PlannedQty,
    string MaterialCode, string MaterialName, string UoM,
    decimal RequiredQtyPerUnit, decimal ScrapFactor);
