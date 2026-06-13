using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Import.Queries.GetImportJobStatus;

public sealed class GetImportJobStatusHandler(IImportRepository repo)
    : IQueryHandler<GetImportJobStatusQuery, ImportJobSummaryDto?>
{
    public async Task<ImportJobSummaryDto?> HandleAsync(
        GetImportJobStatusQuery query, CancellationToken ct = default)
    {
        var job = await repo.GetByIdAsync(query.ImportJobId, ct);
        if (job is null) return null;
        return new ImportJobSummaryDto(
            job.ImportJobId, job.Category, job.Status.ToString(), job.FileName,
            job.TotalRows, job.ValidRows, job.InvalidRows, job.CommittedRows,
            job.ErrorMessage, job.CreatedBy, job.CreatedAt, job.CompletedAt);
    }
}
