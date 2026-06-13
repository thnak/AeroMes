using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Statistics.Queries.GetProductionStatisticsSheetDetail;

public sealed class GetProductionStatisticsSheetDetailHandler(IProductionStatisticsSheetRepository repo)
    : IQueryHandler<GetProductionStatisticsSheetDetailQuery, QueryResult<ProductionStatisticsSheetDetailDto>>
{
    public async Task<QueryResult<ProductionStatisticsSheetDetailDto>> HandleAsync(
        GetProductionStatisticsSheetDetailQuery q, CancellationToken ct)
    {
        var sheet = await repo.GetByIdAsync(q.SheetId, ct);
        if (sheet is null)
            return QueryResult<ProductionStatisticsSheetDetailDto>.NotFound($"Sheet #{q.SheetId} not found.");

        var dto = new ProductionStatisticsSheetDetailDto(
            sheet.SheetId, sheet.SheetNumber, sheet.SheetType.ToString(), sheet.Status.ToString(),
            sheet.POID, sheet.MPOId, sheet.ProductionDate, sheet.Notes, sheet.CreatedAt, sheet.CreatedBy,
            sheet.OutputLines.Select(l => new OutputLineDetailDto(
                l.LineId, l.ProductCode, l.PlannedQty, l.QualifiedQty, l.DefectiveQty, l.DefectCodeId)).ToList(),
            sheet.MaterialLines.Select(l => new MaterialLineDetailDto(
                l.LineId, l.MaterialCode, l.BomStandardQty, l.ActualUsedQty, l.Variance, l.UoMCode, l.VarianceReason)).ToList(),
            sheet.ByProductLines.Select(l => new ByProductLineDetailDto(
                l.LineId, l.ProductCode, l.Qty, l.UoMCode, l.WarehouseCode)).ToList());

        return QueryResult<ProductionStatisticsSheetDetailDto>.Found(dto);
    }
}
