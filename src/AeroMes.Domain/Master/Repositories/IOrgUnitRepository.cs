namespace AeroMes.Domain.Master.Repositories;

public interface IOrgUnitRepository
{
    Task<OrgUnit?> GetByIdAsync(int unitId, CancellationToken ct = default);

    /// <summary>Read-only list, optionally filtered to active units and partial code/name match.</summary>
    Task<IReadOnlyList<OrgUnit>> GetAllAsync(bool activeOnly, string? search, CancellationToken ct = default);

    /// <summary>Tracked load of every unit (active and inactive) for snapshot synchronization.</summary>
    Task<List<OrgUnit>> GetAllForSyncAsync(CancellationToken ct = default);

    Task<bool> IsActiveAsync(int unitId, CancellationToken ct = default);

    Task AddAsync(OrgUnit unit, CancellationToken ct = default);
}
