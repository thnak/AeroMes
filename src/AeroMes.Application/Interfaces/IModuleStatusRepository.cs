namespace AeroMes.Application.Interfaces;

public interface IModuleStatusRepository
{
    Task<int> CountActiveWorkOrdersAsync(CancellationToken ct = default);
    Task<int> CountOpenDowntimeLogsAsync(CancellationToken ct = default);
    Task<int> CountPendingGrnAsync(CancellationToken ct = default);
    Task<int> CountAdaptersDownAsync(CancellationToken ct = default);
}
