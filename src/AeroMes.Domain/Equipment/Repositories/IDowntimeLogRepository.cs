namespace AeroMes.Domain.Equipment.Repositories;

public interface IDowntimeLogRepository
{
    Task<DowntimeLog?> GetByIdAsync(long id, CancellationToken ct = default);

    Task AddAsync(DowntimeLog log, CancellationToken ct = default);

    Task<double> GetTotalDowntimeMinutesAsync(
        int workCenterId,
        string machineCode,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);
}
