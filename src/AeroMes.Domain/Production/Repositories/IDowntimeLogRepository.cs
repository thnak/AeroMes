namespace AeroMes.Domain.Production.Repositories;

public interface IDowntimeLogRepository
{
    Task<DowntimeLog?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<double> GetTotalDowntimeMinutesAsync(
        string machineCode, DateTime from, DateTime to, CancellationToken ct = default);
    Task AddAsync(DowntimeLog entity, CancellationToken ct = default);
}
