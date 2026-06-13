namespace AeroMes.Domain.Production.Repositories;

public record MasterPlanSummaryDto(
    int MasterPlanId,
    string PlanNumber,
    string PlanName,
    string? OrganizationalUnit,
    string Granularity,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string DataSource,
    string Status,
    int LineCount,
    string? CreatedBy,
    DateTime CreatedAt);

public interface IMasterProductionPlanRepository
{
    Task<MasterProductionPlan?> GetByIdAsync(int id, CancellationToken ct);
    Task<MasterProductionPlan?> GetByIdWithLinesAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<MasterPlanSummaryDto>> GetListAsync(string? orgUnit, string? status, DateOnly? from, DateOnly? to, CancellationToken ct);
    Task<bool> ExistsByPlanNumberAsync(string planNumber, CancellationToken ct);
    Task<string> NextPlanNumberAsync(CancellationToken ct);
    Task AddAsync(MasterProductionPlan entity, CancellationToken ct);
    void Remove(MasterProductionPlan entity);
    Task SaveChangesAsync(CancellationToken ct);
}
