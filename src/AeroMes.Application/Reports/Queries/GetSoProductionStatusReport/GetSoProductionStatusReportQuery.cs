using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetSoProductionStatusReport;

public record GetSoProductionStatusReportQuery(
    DateTime? From,
    DateTime? To) : IQuery<IReadOnlyList<SoProductionStatusDto>>;
