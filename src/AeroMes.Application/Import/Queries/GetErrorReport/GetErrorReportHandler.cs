using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Import.Queries.GetErrorReport;

public sealed class GetErrorReportHandler(IImportService importService, IImportRepository repo)
    : IQueryHandler<GetErrorReportQuery, byte[]?>
{
    public async Task<byte[]?> HandleAsync(GetErrorReportQuery query, CancellationToken ct = default)
    {
        var job = await repo.GetByIdAsync(query.ImportJobId, ct);
        if (job is null || string.IsNullOrEmpty(job.ErrorRowsJson))
            return null;

        return await importService.GetErrorReportAsync(job.ErrorRowsJson, ct);
    }
}
