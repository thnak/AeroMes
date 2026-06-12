namespace AeroMes.Domain.Master.Repositories;

public interface IProductionTeamRepository
{
    Task<ProductionTeam?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<ProductionTeam?> GetByCodeWithDetailsAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<ProductionTeam>> GetAllAsync(
        bool activeOnly, string? search, int? orgUnitId, CancellationToken ct = default);
    Task AddAsync(ProductionTeam team, CancellationToken ct = default);
}
