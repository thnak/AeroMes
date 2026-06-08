using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class JobRepository(AppDbContext db) : IJobRepository
{
    public Task<Job?> GetByIdAsync(long id, CancellationToken ct) =>
        db.Jobs.FirstOrDefaultAsync(x => x.JobID == id, ct);

    public Task<Job?> GetActiveJobAsync(int woId, string machineCode, CancellationToken ct) =>
        db.Jobs.FirstOrDefaultAsync(
            x => x.WOID == woId && x.MachineCode == machineCode && x.Status == JobStatus.Active,
            ct);

    public Task AddAsync(Job entity, CancellationToken ct)
    {
        db.Jobs.Add(entity);
        return Task.CompletedTask;
    }
}
