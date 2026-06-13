using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.Queries.GetCopqTrend;

public record GetCopqTrendQuery(int Months, string? ProductCode)
    : IQuery<IReadOnlyList<CopqTrendPointDto>>;
