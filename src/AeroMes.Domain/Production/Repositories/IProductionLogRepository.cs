namespace AeroMes.Domain.Production.Repositories;

public record EmployeeOutputDto(string OperatorId, int QtyOK, int QtyNG, int TotalLogs);
public record ProductOutputDto(string ProductCode, int QtyOK, int QtyNG);

public interface IProductionLogRepository
{
    Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct = default);
    Task<(int ok, int ng)> GetTotalOutputByMachineAsync(
        string machineCode, DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<ProductionLog>> GetByJobIdAsync(long jobId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductionLog>> GetForReportAsync(
        DateTime from, DateTime to, string? workCenterCode, string? machineCode, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeOutputDto>> GetOutputByEmployeeAsync(
        DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<ProductOutputDto>> GetOutputByProductAsync(
        DateTime from, DateTime to, CancellationToken ct = default);
    Task AddAsync(ProductionLog entity, CancellationToken ct = default);
}
