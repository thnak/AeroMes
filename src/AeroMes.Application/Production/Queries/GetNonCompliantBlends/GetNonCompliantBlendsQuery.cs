using AeroMes.Application.Common;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetNonCompliantBlends;

public record GetNonCompliantBlendsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    bool? IsApproved = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<MaterialBlendLogDto>>;
