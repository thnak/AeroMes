using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetProductionReport;

public class GetProductionReportHandler(IProductionLogRepository productionLogRepo)
    : IQueryHandler<GetProductionReportQuery, ProductionReportDto>
{
    public async Task<ProductionReportDto> HandleAsync(GetProductionReportQuery q, CancellationToken ct)
    {
        var logs = await productionLogRepo.GetForReportAsync(
            q.From, q.To, q.WorkCenterCode, q.MachineCode, ct);

        var rows = logs
            .GroupBy(l => new
            {
                Date = l.Timestamp.Date,
                l.Job!.MachineCode,
                WorkCenterCode = l.Job.WorkOrder?.WorkCenter?.WorkCenterCode,
                WorkCenterName = l.Job.WorkOrder?.WorkCenter?.WorkCenterName,
            })
            .Select(g => new ProductionReportRowDto(
                g.Key.Date,
                g.Key.MachineCode,
                g.Key.WorkCenterCode,
                g.Key.WorkCenterName,
                g.Sum(l => l.QtyOK),
                g.Sum(l => l.QtyNG)))
            .OrderBy(r => r.Date).ThenBy(r => r.MachineCode)
            .ToList();

        return new ProductionReportDto(
            q.From, q.To,
            rows.Sum(r => r.QtyOK),
            rows.Sum(r => r.QtyNG),
            rows);
    }
}
