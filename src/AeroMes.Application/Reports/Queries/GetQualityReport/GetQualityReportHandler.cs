using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetQualityReport;

public class GetQualityReportHandler(IDefectDetailRepository defectDetailRepo)
    : IQueryHandler<GetQualityReportQuery, QualityReportDto>
{
    public async Task<QualityReportDto> HandleAsync(GetQualityReportQuery q, CancellationToken ct)
    {
        var details = await defectDetailRepo.GetForReportAsync(q.From, q.To, q.DefectCategory, ct);

        var totalDefects = details.Sum(d => d.Quantity);

        var rows = details
            .GroupBy(d => new
            {
                Code = d.DefectCode!.Code,
                Name = d.DefectCode.DefectName,
                Category = d.DefectCode.DefectCategory,
            })
            .Select(g => new QualityReportRowDto(
                g.Key.Code,
                g.Key.Name,
                g.Key.Category,
                g.Sum(d => d.Quantity),
                totalDefects > 0 ? Math.Round((double)g.Sum(d => d.Quantity) / totalDefects * 100, 2) : 0))
            .OrderByDescending(r => r.TotalQuantity)
            .ToList();

        return new QualityReportDto(q.From, q.To, totalDefects, rows);
    }
}
