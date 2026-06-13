namespace AeroMes.Domain.Wms.Repositories;

public interface IReplenishmentAlertRepository
{
    Task<IReadOnlyList<ReplenishmentAlert>> GetAllAsync(ReplenishmentAlertStatus? status = null, CancellationToken ct = default);
    Task<ReplenishmentAlert?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ReplenishmentAlert?> GetOpenByPolicyAsync(int policyId, CancellationToken ct = default);
    Task AddAsync(ReplenishmentAlert alert, CancellationToken ct = default);
}
