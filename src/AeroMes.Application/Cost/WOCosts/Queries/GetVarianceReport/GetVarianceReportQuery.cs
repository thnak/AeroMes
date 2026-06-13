using AeroMes.Application.Common;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.WOCosts.Queries.GetVarianceReport;

public record GetVarianceReportQuery(
    string? ProductCode, DateOnly? From, DateOnly? To, int? WorkCenterID,
    int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<VarianceReportItemDto>>;
