namespace AeroMes.Domain.Production.Repositories;

public interface IProductionLogRepository
{
    Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct = default);
    Task<(int ok, int ng)> GetTotalOutputByMachineAsync(
        string machineCode, DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<ProductionLog>> GetByJobIdAsync(long jobId, CancellationToken ct = default);
    Task AddAsync(ProductionLog entity, CancellationToken ct = default);
}
