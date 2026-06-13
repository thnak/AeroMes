using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetRegrindUsageSummary;

public record GetRegrindUsageSummaryQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? ResinProductCode = null) : IQuery<IReadOnlyList<RegrindUsageSummaryDto>>;
