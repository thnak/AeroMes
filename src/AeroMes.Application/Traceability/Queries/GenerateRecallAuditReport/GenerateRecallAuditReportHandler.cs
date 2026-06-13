using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GenerateRecallAuditReport;

public sealed class GenerateRecallAuditReportHandler(
    IRecallRepository recallRepository,
    ILotHoldRepository holdRepository)
    : IQueryHandler<GenerateRecallAuditReportQuery, RecallAuditReportDto?>
{
    public async Task<RecallAuditReportDto?> HandleAsync(
        GenerateRecallAuditReportQuery query, CancellationToken ct)
    {
        var detail = await recallRepository.GetDetailAsync(query.RecallID, ct);
        if (detail is null) return null;

        var scope = await recallRepository.GetScopeAsync(query.RecallID, ct);
        var timeline = await recallRepository.GetAuditLogAsync(query.RecallID, ct);

        // Get all holds associated with this recall's scope lots
        var holdIds = scope.Lots
            .Where(l => l.HoldID.HasValue)
            .Select(l => l.HoldID!.Value)
            .ToList();

        var holdHistory = await holdRepository.GetHistoryByRecallAsync(detail.RecallCode, ct);

        return new RecallAuditReportDto(detail, scope, timeline, holdHistory);
    }
}
