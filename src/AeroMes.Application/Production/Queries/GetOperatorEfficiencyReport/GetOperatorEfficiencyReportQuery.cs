using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetOperatorEfficiencyReport;

public record GetOperatorEfficiencyReportQuery(
    string? OperatorID = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IQuery<IReadOnlyList<OperatorEfficiencyDto>>;
