namespace AeroMes.Domain.Production.Repositories;

public record DetailedPlanSummaryDto(
    int DetailPlanId,
    string PlanNumber,
    string PlanName,
    int MasterPlanId,
    string? MasterPlanNumber,
    string? OrganizationalUnit,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string Granularity,
    string Status,
    bool HasProductionOrders,
    int ProductCount,
    string? CreatedBy,
    DateTime CreatedAt);

public interface IDetailedProductionPlanRepository
{
    Task<DetailedProductionPlan?> GetByIdAsync(int id, CancellationToken ct);
    Task<DetailedProductionPlan?> GetByIdWithLinesAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<DetailedPlanSummaryDto>> GetListAsync(int? masterPlanId, string? orgUnit, string? status, CancellationToken ct);
    Task<string> NextPlanNumberAsync(CancellationToken ct);
    Task AddAsync(DetailedProductionPlan entity, CancellationToken ct);
    void Remove(DetailedProductionPlan entity);
    Task SaveChangesAsync(CancellationToken ct);
}
