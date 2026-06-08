namespace AeroMes.Domain.Production.Repositories;

public interface IProductionLogRepository
{
    Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct = default);

    Task<ProductionLog?> GetByIdAsync(long id, CancellationToken ct = default);

    Task AddAsync(ProductionLog log, CancellationToken ct = default);

    Task<(int TotalOk, int TotalNg)> GetTotalOutputByMachineAsync(
        string machineCode,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);
}
