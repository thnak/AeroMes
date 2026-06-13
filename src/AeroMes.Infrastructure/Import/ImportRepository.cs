using AeroMes.Application.Import;
using AeroMes.Domain.Import;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Import;

public sealed class ImportRepository(AppDbContext db) : IImportRepository
{
    public async Task<int> AddAsync(ImportJob job, CancellationToken ct)
    {
        db.ImportJobs.Add(job);
        await db.SaveChangesAsync(ct);
        return job.ImportJobId;
    }

    public Task<ImportJob?> GetByIdAsync(int id, CancellationToken ct)
        => db.ImportJobs.FirstOrDefaultAsync(j => j.ImportJobId == id, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<ImportJobSummaryDto>> GetHistoryAsync(
        int page, int pageSize, CancellationToken ct)
    {
        return await db.ImportJobs.AsNoTracking()
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(j => new ImportJobSummaryDto(
                j.ImportJobId, j.Category, j.Status.ToString(), j.FileName,
                j.TotalRows, j.ValidRows, j.InvalidRows, j.CommittedRows,
                j.ErrorMessage, j.CreatedBy, j.CreatedAt, j.CompletedAt))
            .ToListAsync(ct);
    }
}
