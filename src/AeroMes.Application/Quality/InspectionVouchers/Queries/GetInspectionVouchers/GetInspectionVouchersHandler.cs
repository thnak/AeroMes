using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionVouchers.Queries.GetInspectionVouchers;

public class GetInspectionVouchersHandler(IQualityInspectionVoucherRepository repository)
    : IQueryHandler<GetInspectionVouchersQuery, PagedResult<QualityInspectionVoucherDto>>
{
    public async Task<PagedResult<QualityInspectionVoucherDto>> HandleAsync(
        GetInspectionVouchersQuery query, CancellationToken ct)
    {
        var (items, total) = await repository.GetListAsync(
            query.Status, query.InspectionType,
            query.From, query.To,
            query.Page, query.PageSize, ct);
        return new PagedResult<QualityInspectionVoucherDto>(items, total, query.Page, query.PageSize);
    }
}
