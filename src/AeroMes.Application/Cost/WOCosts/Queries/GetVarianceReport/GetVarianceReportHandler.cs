using AeroMes.Application.Common;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.WOCosts.Queries.GetVarianceReport;

public class GetVarianceReportHandler(IWOCostRepository repository)
    : IQueryHandler<GetVarianceReportQuery, PagedResult<VarianceReportItemDto>>
{
    public async Task<PagedResult<VarianceReportItemDto>> HandleAsync(
        GetVarianceReportQuery query, CancellationToken ct)
    {
        var (items, total) = await repository.GetVarianceReportAsync(
            query.ProductCode, query.From, query.To, query.WorkCenterID,
            query.Page, query.PageSize, ct);
        return new PagedResult<VarianceReportItemDto>(items, total, query.Page, query.PageSize);
    }
}
