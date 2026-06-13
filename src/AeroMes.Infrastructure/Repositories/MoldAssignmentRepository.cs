using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MoldAssignmentRepository(AppDbContext db) : IMoldAssignmentRepository
{
    public Task AddAsync(MoldAssignment assignment, CancellationToken ct)
    {
        db.MoldAssignments.Add(assignment);
        return Task.CompletedTask;
    }

    public Task<MoldAssignment?> GetActiveMountAsync(string moldCode, CancellationToken ct)
        => db.MoldAssignments.FirstOrDefaultAsync(
            a => a.MoldCode == moldCode && a.UnmountedAt == null, ct);

    public async Task<IReadOnlyList<MoldAssignmentDto>> GetHistoryAsync(
        string moldCode, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        var q = db.MoldAssignments.AsNoTracking().Where(a => a.MoldCode == moldCode);
        if (fromDate.HasValue) q = q.Where(a => a.MountedAt >= fromDate.Value);
        if (toDate.HasValue) q = q.Where(a => a.MountedAt <= toDate.Value);

        return await q.OrderByDescending(a => a.MountedAt)
            .Select(a => new MoldAssignmentDto(
                a.AssignmentID, a.MoldCode, a.MachineCode, a.WOID,
                a.MountedAt, a.UnmountedAt, a.MountedBy))
            .ToListAsync(ct);
    }

    public Task AddShotLogAsync(MoldShotLog shotLog, CancellationToken ct)
    {
        db.MoldShotLogs.Add(shotLog);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
