using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Statistics.Queries.GetProductionStatisticsSheets;

public sealed class GetProductionStatisticsSheetsHandler(IProductionStatisticsSheetRepository repo)
    : IQueryHandler<GetProductionStatisticsSheetsQuery, IReadOnlyList<ProductionStatisticsSheetSummaryDto>>
{
    public async Task<IReadOnlyList<ProductionStatisticsSheetSummaryDto>> HandleAsync(
        GetProductionStatisticsSheetsQuery q, CancellationToken ct)
    {
        var list = await repo.GetFilteredAsync(q.POID, q.MPOId, q.SheetType, q.Status, q.From, q.To, ct);
        return list.Select(s => new ProductionStatisticsSheetSummaryDto(
            s.SheetId, s.SheetNumber, s.SheetType.ToString(), s.Status.ToString(),
            s.POID, s.MPOId, s.ProductionDate,
            s.OutputLines.Count,
            s.OutputLines.Sum(l => l.QualifiedQty),
            s.OutputLines.Sum(l => l.DefectiveQty),
            s.CreatedAt, s.CreatedBy)).ToList();
    }
}
