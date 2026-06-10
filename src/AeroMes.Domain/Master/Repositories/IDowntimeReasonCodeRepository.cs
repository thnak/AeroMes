namespace AeroMes.Domain.Master.Repositories;

public interface IDowntimeReasonCodeRepository
{
    Task<DowntimeReasonCode?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<DowntimeReasonCode>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(DowntimeReasonCode entity, CancellationToken ct = default);
}
