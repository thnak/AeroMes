using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.Queries.GetQualityCostSummary;

public record GetQualityCostSummaryQuery(short Year, byte? Month, string? ProductCode)
    : IQuery<IReadOnlyList<QualityCostSummaryDto>>;
