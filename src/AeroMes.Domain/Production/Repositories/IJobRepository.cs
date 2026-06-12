namespace AeroMes.Domain.Production.Repositories;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Job?> GetActiveJobAsync(int woId, string machineCode, CancellationToken ct = default);
    Task<IReadOnlyList<Job>> GetByWoIdAsync(int woId, CancellationToken ct = default);
    Task<IReadOnlyList<Job>> GetFilteredAsync(
        int? woId, string? machineCode, JobStatus? status,
        DateTime? from, DateTime? to, CancellationToken ct = default);
    Task AddAsync(Job entity, CancellationToken ct = default);
}
