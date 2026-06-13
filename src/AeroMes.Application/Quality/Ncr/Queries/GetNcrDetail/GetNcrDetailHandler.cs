using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Queries.GetNcrDetail;

public class GetNcrDetailHandler(INcrRepository repo) : IQueryHandler<GetNcrDetailQuery, NcrDetailDto?>
{
    public async Task<NcrDetailDto?> HandleAsync(GetNcrDetailQuery q, CancellationToken ct)
    {
        var ncr = await repo.GetByIdWithLinesAsync(q.NcrId, ct);
        if (ncr is null)
            return null;

        var defectLines = ncr.DefectLines.Select(l => new NcrDefectLineDto(
            l.LineId,
            l.DefectCodeId,
            l.DefectCode?.Code,
            l.QtyDefective,
            l.Notes)).ToList();

        return new NcrDetailDto(
            ncr.NcrId,
            ncr.NcrNo,
            ncr.Status,
            ncr.Severity,
            ncr.InspectionOrderId,
            ncr.WorkOrderId,
            ncr.ProductCode,
            ncr.LotNumber,
            ncr.QtyAffected,
            ncr.DispositionCode,
            ncr.DispositionSetBy,
            ncr.DispositionSetAt,
            ncr.RootCause,
            ncr.CorrectiveAction,
            ncr.PreventiveAction,
            ncr.AssignedTo,
            ncr.DueDate,
            ncr.ClosedBy,
            ncr.ClosedAt,
            ncr.CreatedAt,
            ncr.CreatedBy,
            defectLines);
    }
}
